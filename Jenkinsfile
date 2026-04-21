pipeline {
    agent any

    environment {
        PROJECT_DIR = '/e/HungNM/NeedApp_BE'
        COMPOSE_FILE = '/e/HungNM/NeedApp_BE/docker-compose.yml'
        GG_CHAT_WEBHOOK_BASE = 'https://chat.googleapis.com/v1/spaces/AAQAyJg5xoU/messages?key=AIzaSyDdI0hCZtE6vySjMm-WEfRq3CPzqKqqsHI&token=bPFiBiw7I06p8wFaPtA0Jr300iUTinPept8BH77KAik'
        JENKINS_URL_PUBLIC = 'http://42.119.236.229:9090'
        CONTAINER_NAME = 'needapp-api'
        GG_CHAT_WEBHOOK = "${GG_CHAT_WEBHOOK_BASE}&threadKey=needapp-be-${BUILD_NUMBER}&messageReplyOption=REPLY_MESSAGE_FALLBACK_TO_NEW_THREAD"
    }

    triggers {
        githubPush()
    }

    stages {
        stage('Checkout') {
            steps {
                sh """
                    curl -s -X POST '${GG_CHAT_WEBHOOK}' \
                        -H 'Content-Type: application/json' \
                        -d '{"text": "🚀 *NeedApp BE - Deployment Started*\\nBuild: #${BUILD_NUMBER} | Branch: ${GIT_BRANCH}\\nView log: ${JENKINS_URL_PUBLIC}/job/needapp-be/${BUILD_NUMBER}/console"}'
                """
                checkout scm
            }
        }

        stage('Sync Code') {
            steps {
                sh """
                    MSG=\$(git log -1 --pretty=%s | cut -c1-60)
                    curl -s -X POST '${GG_CHAT_WEBHOOK}' \
                        -H 'Content-Type: application/json' \
                        -d "{\\"text\\": \\"🔄 *[NeedApp BE - 1/4] Sync Code...*\\\\nCommit: ${GIT_COMMIT.take(7)} - \$MSG\\"}"
                """
                sh 'docker rm -f ${CONTAINER_NAME} 2>/dev/null || true'
                sh 'tar cf - --exclude=.git --exclude=.env -C $WORKSPACE . | tar xf - -C $PROJECT_DIR'
            }
        }

        stage('Build') {
            steps {
                sh """
                    curl -s -X POST '${GG_CHAT_WEBHOOK}' \
                        -H 'Content-Type: application/json' \
                        -d '{"text": "🔨 *[NeedApp BE - 2/4] Building Docker image...*"}'
                """
                dir(PROJECT_DIR) {
                    sh 'docker compose -f ${COMPOSE_FILE} build api'
                }
            }
        }

        stage('Deploy') {
            steps {
                sh """
                    curl -s -X POST '${GG_CHAT_WEBHOOK}' \
                        -H 'Content-Type: application/json' \
                        -d '{"text": "📦 *[NeedApp BE - 3/4] Deploying container...*"}'
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
                    curl -s -X POST '${GG_CHAT_WEBHOOK}' \
                        -H 'Content-Type: application/json' \
                        -d '{"text": "🩺 *[NeedApp BE - 4/4] Running health check...*"}'
                """
                sh '''
                    sleep 10
                    for i in $(seq 1 12); do
                        STATUS=$(docker inspect --format="{{.State.Status}}" ${CONTAINER_NAME} 2>/dev/null || echo "not_found")
                        if [ "$STATUS" = "running" ]; then
                            HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://host.docker.internal:8081/health 2>/dev/null)
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
                curl -s -X POST '${GG_CHAT_WEBHOOK}' \
                    -H 'Content-Type: application/json' \
                    -d '{"text": "✅ *NeedApp BE - Deployment Successful*\\nBuild: #${BUILD_NUMBER} | Commit: ${GIT_COMMIT.take(7)}\\nURL: http://localhost:8081\\nView log: ${JENKINS_URL_PUBLIC}/job/needapp-be/${BUILD_NUMBER}/console"}'
            """
        }
        failure {
            sh """
                curl -s -X POST '${GG_CHAT_WEBHOOK}' \
                    -H 'Content-Type: application/json' \
                    -d '{"text": "❌ *NeedApp BE - Deployment Failed*\\nBuild: #${BUILD_NUMBER} | Commit: ${GIT_COMMIT.take(7)}\\nView log: ${JENKINS_URL_PUBLIC}/job/needapp-be/${BUILD_NUMBER}/console"}'
            """
        }
        aborted {
            sh """
                curl -s -X POST '${GG_CHAT_WEBHOOK}' \
                    -H 'Content-Type: application/json' \
                    -d '{"text": "⚠️ *NeedApp BE - Deployment Aborted*\\nBuild: #${BUILD_NUMBER}\\nView log: ${JENKINS_URL_PUBLIC}/job/needapp-be/${BUILD_NUMBER}/console"}'
            """
        }
    }
}
