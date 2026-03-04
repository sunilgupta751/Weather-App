pipeline {
    agent any

    environment {
        ACR_URL = "acrlearn001.azurecr.io"
        IMAGE_NAME = "weatherapp"
        ACR_CRED_ID = "azure-acr-creds" // Jo aapne Jenkins mein save kiya hai
    }

    stages {
        stage('Checkout Code') {
            steps {
                // Multibranch pipeline apne aap sahi branch checkout karti hai
                checkout scm
            }
        }

        stage('Build & Push to ACR') {
            steps {
                script {
                    docker.withRegistry("https://${ACR_URL}", "${ACR_CRED_ID}") {
                        // Hum WeatherApps folder ke andar jaakar build kar rahe hain
                        dir('WeatherApps') {
                            def appImage = docker.build("${ACR_URL}/${IMAGE_NAME}:${env.BRANCH_NAME}-${env.BUILD_NUMBER}", ".")
                            appImage.push()
                            appImage.push("latest")
                        }
                    }
                }
            }
        }

        stage('Deploy to QA') {
            when { branch 'dev' } // Sirf dev branch par chalega
            steps {
                echo "Deploying to QA Environment automatically..."
                // Yahan aapki QA deployment command aayegi
            }
        }

        stage('Approval for Staging') {
            when { branch 'staging' } // Manager Approval sirf main (PR merge) par
            steps {
                input message: "QA Testing OK? Staging/UAT pe deploy karein?", ok: "Approve"
            }
        }

        stage('Deploy to Staging') {
            when { branch 'staging' }
            steps {
                echo "Deploying to Staging Environment..."
            }
        }
    }

    post {
        always {
            cleanWs() // Workspace cleanup professional practice hai
        }
    }
}
