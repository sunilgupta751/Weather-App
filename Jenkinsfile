pipeline {
    agent any

    environment {
        // --- Common Config ---
        IMAGE_NAME      = "weatherapp"
        GIT_COMMIT_REV  = sh(script: "git rev-parse --short HEAD", returnStatus: false, returnStdout: true).trim()
        // Server Details (Inhe aap apne IP se badal dena)
        DEV_SERVER_IP   = "20.96.26.236" 
        STAGING_SERVER_IP = "23.x.x.x"
        PROD_SERVER_IP  = "52.x.x.x"
        SSH_CRED_ID     = "server-ssh-creds" // Jenkins mein save ki hui SSH ID
        VM_USER         = "azureuser"
        // --- Secrets (Jenkins Credentials mein save honi chahiye) ---
        RABBIT_PASS     = credentials('rabbitmq-password-id')
    }
  
    stages {
        stage('Setup Environment') {
            steps {
                script {
                    // Branch ke hisaab se URL aur Credentials switch karna
                    if (env.BRANCH_NAME == 'prod') {
                        env.ACR_URL = "prod-acr-url.azurecr.io"
                        env.ACR_CRED_ID = "acr-prod-creds"
                    } else if (env.BRANCH_NAME == 'staging') {
                        env.ACR_URL = "staging-acr-url.azurecr.io"
                        env.ACR_CRED_ID = "acr-staging-creds"
                    } else {
                        // Default is Dev
                        env.ACR_URL = "acrlearn001.azurecr.io"
                        env.ACR_CRED_ID = "acr-credentials-id-jenkins"
                    }
                    
                    env.UNIQUE_TAG = "${env.BRANCH_NAME}-build${env.BUILD_NUMBER}-${GIT_COMMIT_REV}"
                }
            }
        }

        stage('Build & Push to ACR') {
            steps {
                script {
                    echo "🚀 Target Registry: ${env.ACR_URL}"
                    
                    docker.withRegistry("https://${env.ACR_URL}", "${env.ACR_CRED_ID}") {
                        dir('WeatherApps') {
                            def appImage = docker.build("${env.ACR_URL}/${IMAGE_NAME}:${env.UNIQUE_TAG}", ".")
                            
                            // 1. Unique Tag Push (For Rollbacks)
                            appImage.push()
                            
                            // 2. Environment specific latest tag
                            appImage.push("${env.BRANCH_NAME}-latest")
                        }
                    }
                }
            }
        }

        // --- Deployment Stages (Baki stages pehle jaise rahengi) ---
        stage('Deploy to Dev') {
             when { branch 'dev' }
            steps {
                script {
                    def targetIP = ""
                    def targetPort = ""
                    def vmPath = "/home/${VM_USER}/weatherapp-${env.BRANCH_NAME}"
                    
                    if (env.BRANCH_NAME == 'prod') {
                        targetIP = PROD_SERVER_IP
                        targetPort = "80"
                    } else if (env.BRANCH_NAME == 'staging') {
                        targetIP = STAGING_SERVER_IP
                        targetPort = "8081"
                    } else {
                        targetIP = DEV_SERVER_IP
                        targetPort = "8081"
                    }

                    echo "🚀 Deploying to ${env.BRANCH_NAME} server at ${targetIP}..."

                    // SSH ke zariye server par command chalana
                    sshagent(["${SSH_CRED_ID}"]) {
                        
                        // VM par folder banana agar nahi hai
                        sh "ssh -o StrictHostKeyChecking=no ${VM_USER}@${VM_IP} 'mkdir -p ${vmPath}'"

                        // Repo se docker-compose.yml file VM par bhejna
                        sh "scp -o StrictHostKeyChecking=no WeatherApps/docker-compose.yml ${VM_USER}@${VM_IP}:${vmPath}/"

                        // VM ke andar ghus kar commands chalana
                        sh """
                            ssh -o StrictHostKeyChecking=no ${VM_USER}@${VM_IP} "
                                cd ${vmPath}
                                
                                # ACR Login (Internal login for docker-compose)
                                sudo az acr login --name acrlearn001

                                # .env file banana (Secrets protect karne ka best tarika)
                                echo 'ACR_URL=${ACR_URL}' > .env
                                echo 'IMAGE_NAME=${IMAGE_NAME}' >> .env
                                echo 'DOCKER_TAG=${UNIQUE_TAG}' >> .env
                                echo 'HOST_PORT=${targetPort}' >> .env
                                echo 'RABBIT_USER=admin' >> .env
                                echo 'RABBIT_PASS=${RABBIT_PASS}' >> .env

                                # Compose Magic
                                sudo docker-compose pull
                                sudo docker-compose down --remove-orphans
                                sudo docker-compose up -d
                                
                                echo '🎉 Deployment Successful on Port ${targetPort}!'
                            "
                        """
                    }
                }
            }
        }
        
        
       stage('Approval for Staging') {
            when { branch 'staging' } // Manager Approval sirf main (PR merge) par
            steps {
                input message: "Dev Testing OK? Staging/UAT pe deploy karein?", ok: "Approve"
            }
        }

    }

    post {
        always { cleanWs() }
    }
}
