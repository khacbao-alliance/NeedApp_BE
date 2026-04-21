pipeline {
    agent any

    environment {
        PROJECT_DIR = '/e/HungNM/NeedApp_BE'
        COMPOSE_FILE = '/e/HungNM/NeedApp_BE/docker-compose.yml'
        GG_CHAT_WEBHOOK = 'https://chat.googleapis.com/v1/spaces/AAQAyJg5xoU/messages?key=AIzaSyDdI0hCZtE6vySjMm-WEfRq3CPzqKqqsHI&token=bPFiBiw7I06p8wFaPtA0Jr300iUTinPept8BH77KAik'
        JENKINS_URL_PUBLIC = 'http://42.119.236.229:9090'
        CONTAINER_NAME = 'needapp-api'
        APP_VERSION = '1.1'
        THREAD_FILE = "/tmp/gchat_thread_needapp_be_${BUILD_NUMBER}"
    }

    triggers {
        githubPush()
    }

    stages {
        stage('Checkout') {
            steps {
                sh """
                    RESPONSE=\$(curl -s -X POST '${GG_CHAT_WEBHOOK}&messageReplyOption=REPLY_MESSAGE_FALLBACK_TO_NEW_THREAD' \
                        -H 'Content-Type: application/json' \
                        -d '{"text": "🚀 *NeedApp BE - Deployment Started*\\nBuild: #${BUILD_NUMBER}\\nBranch: ${GIT_BRANCH}\\nView log: ${JENKINS_URL_PUBLIC}/job/needapp-be/${BUILD_NUMBER}/console"}')
                    echo \$RESPONSE | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['thread']['name'])" > ${THREAD_FILE} 2>/dev/null || true
                """
                checkout scm
            }
        }

        stage('Sync Code') {
            steps {
                sh """
                    THREAD_NAME=\$(cat ${THREAD_FILE} 2>/dev/null || echo "")
                    MSG=\$(git log -1 --pretty=%s | cut -c1-60)
                    BODY="{\\"text\\": \\"🔄 *[NeedApp BE - 1/4] Sync Code...*\\\\nCommit: ${GIT_COMMIT.take(7)} - \$MSG\\"}"
                    if [ -n "\$THREAD_NAME" ]; then
                        BODY="{\\"text\\": \\"🔄 *[NeedApp BE - 1/4] Sync Code...*\\\\nCommit: ${GIT_COMMIT.take(7)} - \$MSG\\", \\"thread\\": {\\"name\\": \\"\$THREAD_NAME\\"}}"
                    fi
                    curl -s -X POST '${GG_CHAT_WEBHOOK}&messageReplyOption=REPLY_MESSAGE_FALLBACK_TO_NEW_THREAD' \
                        -H 'Content-Type: application/json' \
                        -d "\$BODY"
                """
                sh 'docker rm -f ${CONTAINER_NAME} 2>/dev/null || true'
                sh 'tar cf - --exclude=.git --exclude=.env -C $WORKSPACE . | tar xf - -C $PROJECT_DIR'
            }
        }

        stage('Build') {
            steps {
                sh """
                    THREAD_NAME=\$(cat ${THREAD_FILE} 2>/dev/null || echo "")
                    BODY='{"text": "🔨 *[NeedApp BE - 2/4] Building Docker image...*"}'
                    if [ -n "\$THREAD_NAME" ]; then
                        BODY="{\\"text\\": \\"🔨 *[NeedApp BE - 2/4] Building Docker image...*\\", \\"thread\\": {\\"name\\": \\"\$THREAD_NAME\\"}}"
                    fi
                    curl -s -X POST '${GG_CHAT_WEBHOOK}&messageReplyOption=REPLY_MESSAGE_FALLBACK_TO_NEW_THREAD' \
                        -H 'Content-Type: application/json' \
                        -d "\$BODY"
                """
                dir(PROJECT_DIR) {
                    sh 'docker compose -f ${COMPOSE_FILE} build api'
                }
            }
        }

        stage('Deploy') {
            steps {
                sh """
                    THREAD_NAME=\$(cat ${THREAD_FILE} 2>/dev/null || echo "")
                    BODY='{"text": "📦 *[NeedApp BE - 3/4] Deploying container...*"}'
                    if [ -n "\$THREAD_NAME" ]; then
                        BODY="{\\"text\\": \\"📦 *[NeedApp BE - 3/4] Deploying container...*\\", \\"thread\\": {\\"name\\": \\"\$THREAD_NAME\\"}}"
                    fi
                    curl -s -X POST '${GG_CHAT_WEBHOOK}&messageReplyOption=REPLY_MESSAGE_FALLBACK_TO_NEW_THREAD' \
                        -H 'Content-Type: application/json' \
                        -d "\$BODY"
                """
                dir(PROJECT_DIR) {
                    sh 'docker compose -f ${COMPOSE_FILE} down --remove-orphans 2>/dev/null || true'
                    sh 'docker compose -f ${COMPOSE_FILE} up -d'
                }
            }
        }

        stage('Health Check') {
            steps {
                sh """
                    THREAD_NAME=\$(cat ${THREAD_FILE} 2>/dev/null || echo "")
                    BODY='{"text": "🩺 *[NeedApp BE - 4/4] Running health check...*"}'
                    if [ -n "\$THREAD_NAME" ]; then
                        BODY="{\\"text\\": \\"🩺 *[NeedApp BE - 4/4] Running health check...*\\", \\"thread\\": {\\"name\\": \\"\$THREAD_NAME\\"}}"
                    fi
                    curl -s -X POST '${GG_CHAT_WEBHOOK}&messageReplyOption=REPLY_MESSAGE_FALLBACK_TO_NEW_THREAD' \
                        -H 'Content-Type: application/json' \
                        -d "\$BODY"
                """
                sh '''
                    echo "Waiting for container to start..."
                    sleep 10
                    for i in $(seq 1 12); do
                        STATUS=$(docker inspect --format="{{.State.Status}}" ${CONTAINER_NAME} 2>/dev/null || echo "not_found")
                        if [ "$STATUS" = "running" ]; then
                            HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8081/health 2>/dev/null || echo "000")
                            if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "404" ]; then
                                echo "Container is running (HTTP $HTTP_CODE)"
                                exit 0
                            fi
                        fi
                        echo "Attempt $i/12: waiting..."
                        sleep 5
                    done
                    echo "Health check failed"
                    docker logs ${CONTAINER_NAME} --tail 50
                    exit 1
                '''
            }
        }
    }

    post {
        success {
            sh """
                THREAD_NAME=\$(cat ${THREAD_FILE} 2>/dev/null || echo "")
                BODY='{"text": "✅ *NeedApp BE - Deployment Successful*\\nBuild: #${BUILD_NUMBER}\\nCommit: ${GIT_COMMIT.take(7)}\\nURL: http://localhost:8081\\nView log: ${JENKINS_URL_PUBLIC}/job/needapp-be/${BUILD_NUMBER}/console"}'
                if [ -n "\$THREAD_NAME" ]; then
                    BODY="{\\"text\\": \\"✅ *NeedApp BE - Deployment Successful*\\\\nBuild: #${BUILD_NUMBER}\\\\nCommit: ${GIT_COMMIT.take(7)}\\\\nURL: http://localhost:8081\\\\nView log: ${JENKINS_URL_PUBLIC}/job/needapp-be/${BUILD_NUMBER}/console\\", \\"thread\\": {\\"name\\": \\"\$THREAD_NAME\\"}}"
                fi
                curl -s -X POST '${GG_CHAT_WEBHOOK}&messageReplyOption=REPLY_MESSAGE_FALLBACK_TO_NEW_THREAD' \
                    -H 'Content-Type: application/json' \
                    -d "\$BODY"
                rm -f ${THREAD_FILE}
            """
        }
        failure {
            sh """
                THREAD_NAME=\$(cat ${THREAD_FILE} 2>/dev/null || echo "")
                BODY='{"text": "❌ *NeedApp BE - Deployment Failed*\\nBuild: #${BUILD_NUMBER}\\nCommit: ${GIT_COMMIT.take(7)}\\nView log: ${JENKINS_URL_PUBLIC}/job/needapp-be/${BUILD_NUMBER}/console"}'
                if [ -n "\$THREAD_NAME" ]; then
                    BODY="{\\"text\\": \\"❌ *NeedApp BE - Deployment Failed*\\\\nBuild: #${BUILD_NUMBER}\\\\nCommit: ${GIT_COMMIT.take(7)}\\\\nView log: ${JENKINS_URL_PUBLIC}/job/needapp-be/${BUILD_NUMBER}/console\\", \\"thread\\": {\\"name\\": \\"\$THREAD_NAME\\"}}"
                fi
                curl -s -X POST '${GG_CHAT_WEBHOOK}&messageReplyOption=REPLY_MESSAGE_FALLBACK_TO_NEW_THREAD' \
                    -H 'Content-Type: application/json' \
                    -d "\$BODY"
                rm -f ${THREAD_FILE}
            """
        }
        aborted {
            sh """
                THREAD_NAME=\$(cat ${THREAD_FILE} 2>/dev/null || echo "")
                BODY='{"text": "⚠️ *NeedApp BE - Deployment Aborted*\\nBuild: #${BUILD_NUMBER}\\nView log: ${JENKINS_URL_PUBLIC}/job/needapp-be/${BUILD_NUMBER}/console"}'
                if [ -n "\$THREAD_NAME" ]; then
                    BODY="{\\"text\\": \\"⚠️ *NeedApp BE - Deployment Aborted*\\\\nBuild: #${BUILD_NUMBER}\\\\nView log: ${JENKINS_URL_PUBLIC}/job/needapp-be/${BUILD_NUMBER}/console\\", \\"thread\\": {\\"name\\": \\"\$THREAD_NAME\\"}}"
                fi
                curl -s -X POST '${GG_CHAT_WEBHOOK}&messageReplyOption=REPLY_MESSAGE_FALLBACK_TO_NEW_THREAD' \
                    -H 'Content-Type: application/json' \
                    -d "\$BODY"
                rm -f ${THREAD_FILE}
            """
        }
    }
}
