pipeline {
    agent any

    environment {
        // --- Common Config ---
        IMAGE_NAME      = "weatherapp"
        GIT_COMMIT_REV  = sh(script: "git rev-parse --short HEAD", returnStatus: false, returnStdout: true).trim()
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
            steps { echo "✅ Deploying ${env.UNIQUE_TAG} to Dev ACR/Env..." }
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
