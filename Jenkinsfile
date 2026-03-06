pipeline {
    agent any

    environment {
        IMAGE_NAME      = "weatherapp"
        GIT_COMMIT_REV  = sh(script: "git rev-parse --short HEAD", returnStatus: false, returnStdout: true).trim()
        DEV_SERVER_IP   = "20.96.26.236" 
        SSH_CRED_ID     = "server-ssh-creds"
        VM_USER         = "azureuser"
        RABBIT_PASS     = credentials('rabbitmq-password-id')
    }
  
    stages {
        stage('Setup Environment') {
            steps {
                script {
                    if (env.BRANCH_NAME == 'prod') {
                        env.ACR_URL = "prod-acr-url.azurecr.io"
                        env.ACR_CRED_ID = "acr-prod-creds"
                    } else if (env.BRANCH_NAME == 'staging') {
                        env.ACR_URL = "staging-acr-url.azurecr.io"
                        env.ACR_CRED_ID = "acr-staging-creds"
                    } else {
                        env.ACR_URL = "acrlearn001.azurecr.io"
                        // Yeh aapka Service Principal Credentials ID hona chahiye Jenkins mein
                        env.ACR_CRED_ID = "acr-credentials-id-jenkins" 
                    }
                    env.UNIQUE_TAG = "${env.BRANCH_NAME}-build${env.BUILD_NUMBER}-${GIT_COMMIT_REV}"
                }
            }
        }

        stage('Build & Push to ACR') {
            steps {
                script {
                    docker.withRegistry("https://${env.ACR_URL}", "${env.ACR_CRED_ID}") {
                        dir('WeatherApps') {
                            def appImage = docker.build("${env.ACR_URL}/${IMAGE_NAME}:${env.UNIQUE_TAG}", ".")
                            appImage.push()
                            appImage.push("${env.BRANCH_NAME}-latest")
                        }
                    }
                }
            }
        }

        stage('Deploy to Dev') {
            when { branch 'dev' }
            steps {
                // IMPORTANT: Service Principal credentials yahan load karne honge
                withCredentials([usernamePassword(credentialsId: env.ACR_CRED_ID, usernameVariable: 'ACR_USR', passwordVariable: 'ACR_PSW')]) {
                    script {
                        def vmPath = "/home/${VM_USER}/weatherapp-${env.BRANCH_NAME}"
                        def targetPort = "8081"

                        sshagent(["${SSH_CRED_ID}"]) {
                            sh "ssh -o StrictHostKeyChecking=no ${VM_USER}@${DEV_SERVER_IP} 'mkdir -p ${vmPath}'"
                            sh "scp -o StrictHostKeyChecking=no WeatherApps/docker-compose.yml ${VM_USER}@${DEV_SERVER_IP}:${vmPath}/"

                            sh """
                                ssh -o StrictHostKeyChecking=no ${VM_USER}@${DEV_SERVER_IP} << 'EOF'
                                    set -e
                                    cd ${vmPath}

                                    # Service Principal ke throught Login (Instead of az acr login)
                                    echo "${ACR_PSW}" | sudo docker login ${env.ACR_URL} -u "${ACR_USR}" --password-stdin

                                    # .env file (Variable name fixed: IMAGE_TAG)
                                    echo "ACR_URL=${env.ACR_URL}" > .env
                                    echo "IMAGE_NAME=${IMAGE_NAME}" >> .env
                                    echo "IMAGE_TAG=${env.UNIQUE_TAG}" >> .env
                                    echo "HOST_PORT=${targetPort}" >> .env
                                    echo "RABBIT_USER=admin" >> .env
                                    echo "RABBIT_PASS=${env.RABBIT_PASS}" >> .env

                                    sudo docker compose pull
                                    sudo docker compose down --remove-orphans
                                    sudo docker compose up -d
                                    
                                    echo '🚀 Deployment Successful with Service Principal on Port ${targetPort}!'
EOF
                            """
                        }
                    }
                }
            }
        }
    }

    post {
        always { cleanWs() }
    }
}
