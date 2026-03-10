@Library("shared-libraries") _
pipeline {
    agent {
        label 'agentdotnet' 
    }
    environment {
        ACR_URL = "acrlearn001.azurecr.io"
        IMAGE_NAME = "weatherapp"
        ACR_CRED_ID = "acr-credentials-id-jenkins"
        SSH_CRED_ID = "server-ssh-creds"
        RABBIT_CRED_ID = "rabbitmq-password-id"
    }

    stages {
        stage('Initialize & Build') {
            steps {
                script {
                    // Unique Tagging
                    env.GIT_SHA = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                    env.DOCKER_TAG = "${env.BRANCH_NAME}-${env.BUILD_NUMBER}-${env.GIT_SHA}"

                    // Azure Login: Using Service Principal
                    withCredentials([azureServicePrincipal('acr-credentials-id-jenkins')]) {
                        sh 'az login --service-principal -u $AZURE_CLIENT_ID -p $AZURE_CLIENT_SECRET -t $AZURE_TENANT_ID'
                        
                        dir('WeatherApps') {
                            echo "🚀 MNC Standard: Offloading build to Azure ACR Task..."
                            sh "az acr build --registry acrlearn001 --image ${env.IMAGE_NAME}:${env.DOCKER_TAG} ."
                        }
                    } // withCredentials band
                } // script block band
            } // steps band
        } // stage band

        stage('Deploy to DEV') {
            when { branch 'dev' }
            steps { 
                deployToVM("20.96.26.236", "8081", "dev") 
            }
        }

        stage('Deploy to STAGING') {
            when { branch 'staging' }
            steps {
                input message: "Approve deployment to STAGING?", ok: "Deploy"
                deployToVM("23.x.x.x", "8081", "staging")
            }
        }

        stage('Deploy to PROD') {
            when { anyOf { branch 'main'; branch 'prod' } }
            steps {
                input message: "🚀 Ready for Production?", ok: "Deploy"
                deployToVM("52.x.x.x", "80", "prod")
            }
        }
    } // stages band

    post {
        success {
            echo "✅ Deployment Successful!"
        }
        failure {
            echo "🔴 Deployment Failed! Check logs immediately."
        }
        always {
            echo "🧹 Cleaning up workspace..."
            cleanWs()
        }
    }
} // pipeline band
