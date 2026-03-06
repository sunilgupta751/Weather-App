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
		withCredentials([usernamePassword(credentialsId: env.ACR_CRED_ID, usernameVariable: 'ACR_USR', passwordVariable: 'ACR_PSW')]) {
                    // SSH ke zariye server par command chalana
                    sshagent(["${SSH_CRED_ID}"]) {
                        // VM par folder banana
                        sh "ssh -o StrictHostKeyChecking=no ${VM_USER}@${DEV_SERVER_IP} 'mkdir -p ${vmPath}'"
                    
                        // docker-compose file bhejna
                        sh "scp -o StrictHostKeyChecking=no WeatherApps/docker-compose.yml ${VM_USER}@${DEV_SERVER_IP}:${vmPath}/"
                    
                        // VM ke andar deployment commands
                        // Dhyaan dein: Yahan humne \$ use kiya hai bash variables ke liye aur ${} Jenkins variables ke liye
                        sh """
                            ssh -o StrictHostKeyChecking=no ${VM_USER}@${DEV_SERVER_IP} << 'EOF'
                                set -e  # Agar koi error aaye toh script wahin ruk jaye
                                cd ${vmPath}
                    
                                # 1. ACR Login (Admin credentials use karein ya Jenkins se pass karein)
                                # Agar az login kaam nahi kar raha, toh manual docker login karein
                                sudo az acr login --name acrlearn001 || echo "Az login failed, trying fallback..."
                    
                                # 2. .env file (Yahan IMAGE_TAG use karein kyunki compose wahi maang raha hai)
                                echo "ACR_URL=${ACR_URL}" > .env
                                echo "IMAGE_NAME=${IMAGE_NAME}" >> .env
                                echo "IMAGE_TAG=${UNIQUE_TAG}" >> .env
                                echo "HOST_PORT=${targetPort}" >> .env
                                echo "RABBIT_USER=admin" >> .env
                                echo "RABBIT_PASS=${RABBIT_PASS}" >> .env
                    
                                # 3. Docker Compose execution
                                echo "Pulling latest images..."
                                sudo docker compose pull
                                
                                echo "Restarting containers..."
                                sudo docker compose down --remove-orphans
                                sudo docker compose up -d
                    
                                echo '🚀 Real Deployment Successful on Port ${targetPort}!'
                    EOF
                        """
                    }
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
