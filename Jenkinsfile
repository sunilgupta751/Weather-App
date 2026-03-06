pipeline {
    agent any

    environment {
        ACR_URL         = "acrlearn001.azurecr.io"
        IMAGE_NAME      = "weatherapp"
        SSH_CRED_ID     = "server-ssh-creds"
        ACR_CRED_ID     = "acr-credentials-id-jenkins" 
        RABBIT_CRED_ID  = "rabbitmq-password-id"
        GIT_SHA         = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
    }

    stages {
        stage('Initialize Environment') {
            steps {
                script {
                    // Branch identification
                    if (env.BRANCH_NAME == 'prod' || env.BRANCH_NAME == 'main') {
                        env.TARGET_IP   = "52.x.x.x" 
                        env.TARGET_PORT = "80"
                        env.ENV_NAME    = "PRODUCTION"
                    } else if (env.BRANCH_NAME == 'staging') {
                        env.TARGET_IP   = "23.x.x.x" 
                        env.TARGET_PORT = "8081"
                        env.ENV_NAME    = "STAGING"
                    } else {
                        env.TARGET_IP   = "20.96.26.236"
                        env.TARGET_PORT = "8081"
                        env.ENV_NAME    = "DEVELOPMENT"
                    }
                    env.DOCKER_TAG = "${env.ENV_NAME.toLowerCase()}-${env.BUILD_NUMBER}-${env.GIT_SHA}"
                    
                    // Dashboard par build ka naam badalna (Meaningful Name)
                    currentBuild.displayName = "#${env.BUILD_NUMBER} [${env.ENV_NAME}]"
                    currentBuild.description = "Deploying ${env.DOCKER_TAG} to ${env.TARGET_IP}"
                }
            }
        }

        stage('Build & Push to ACR') {
            steps {
                script {
                    echo "🛠 Building image for ${env.ENV_NAME}..."
                    docker.withRegistry("https://${env.ACR_URL}", "${env.ACR_CRED_ID}") {
                        dir('WeatherApps') {
                            def appImage = docker.build("${env.ACR_URL}/${IMAGE_NAME}:${env.DOCKER_TAG}", "--pull .")
                            appImage.push()
                            appImage.push("${env.ENV_NAME.toLowerCase()}-latest")
                        }
                    }
                }
            }
        }

        stage('Approval Gate') {
            // Dashboard par ye stage tabhi dikhegi jab 'staging' ya 'prod' branch hogi
            when { 
                expression { env.BRANCH_NAME == 'staging' || env.BRANCH_NAME == 'prod' || env.BRANCH_NAME == 'main' } 
            }
            steps {
                echo "⏳ Waiting for Approval to deploy on ${env.ENV_NAME}..."
                input message: "🚀 Deploying to ${env.ENV_NAME}. Continue?", ok: "Approve"
            }
        }

        // --- Meaningful Stage Name ---
        stage('Deploy to Server') {
            steps {
                script {
                    // Dashboard par stage ka naam dynamic dikhega
                    echo "🚢 Deploying to ${env.ENV_NAME} Server..."
                    
                    withCredentials([
                        usernamePassword(credentialsId: env.ACR_CRED_ID, usernameVariable: 'ACR_USR', passwordVariable: 'ACR_PSW'),
                        string(credentialsId: env.RABBIT_CRED_ID, variable: 'RABBIT_PASS')
                    ]) {
                        sshagent(["${SSH_CRED_ID}"]) {
                            def vmPath = "/home/azureuser/deployments/${IMAGE_NAME}-${env.ENV_NAME.toLowerCase()}"
                            
                            sh """
                                ssh -o StrictHostKeyChecking=no azureuser@${env.TARGET_IP} << 'EOF'
                                    set -e
                                    mkdir -p ${vmPath}
                                    cd ${vmPath}

                                    echo "${ACR_PSW}" | sudo docker login ${env.ACR_URL} -u "${ACR_USR}" --password-stdin

                                    echo "ACR_URL=${env.ACR_URL}" > .env
                                    echo "IMAGE_NAME=${IMAGE_NAME}" >> .env
                                    echo "IMAGE_TAG=${env.DOCKER_TAG}" >> .env
                                    echo "RABBIT_PASS=${RABBIT_PASS}" >> .env
                                    echo "HOST_PORT=${env.TARGET_PORT}" >> .env

                                    sudo docker compose pull
                                    sudo docker compose down --remove-orphans
                                    sudo docker compose up -d --force-recreate

                                    # Quick Health Check
                                    sleep 10
                                    curl -f http://localhost:${env.TARGET_PORT}/ || (echo "Health check failed" && exit 1)
EOF
                            """
                        }
                    }
                }
            }
        }
    }

    post {
        success {
            echo "✅ Successfully deployed to ${env.ENV_NAME}!"
        }
        always {
            cleanWs()
        }
    }
}
