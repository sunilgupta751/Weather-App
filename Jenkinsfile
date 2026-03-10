
@Library("shared-libraries") _
pipeline {
    agent {
        label 'dotnet-build-aci-agent' 
    }
    environment {
        ACR_URL = "acrlearn001.azurecr.io"
        IMAGE_NAME = "weatherapp"
        ACR_CRED_ID = "acr-credentials-id-jenkins"
        SSH_CRED_ID = "server-ssh-creds"
        RABBIT_CRED_ID = "rabbitmq-password-id"
        //Ye line Git se aapke latest commit ka unique ID (jise SHA ya Hash kehte hain) mangti hai aur use chota karke (short) ek variable mein store kar deti hai.
        GIT_SHA = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
    }

    stages {
        stage('Initialize & Build') {
            steps {
                script {
             
                    /*
                    env.DOCKER_TAG = "${env.BRANCH_NAME}-${env.BUILD_NUMBER}-${env.GIT_SHA}"
                    currentBuild.displayName = "#${env.BUILD_NUMBER} [${env.BRANCH_NAME.toUpperCase()}]"
                    docker.withRegistry("https://${env.ACR_URL}", "${env.ACR_CRED_ID}") {
                        dir('WeatherApps') {
                            def appImage = docker.build("${env.ACR_URL}/${IMAGE_NAME}:${env.DOCKER_TAG}")
                            appImage.push()
                        }
                    }*/
                    // Unique Tagging
            // Unique Tagging
            env.GIT_SHA = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
            env.DOCKER_TAG = "${env.BRANCH_NAME}-${env.BUILD_NUMBER}-${env.GIT_SHA}"

            // Azure Login: Ismein wahi Credential ID use karo jo Jenkins mein save hai
            withCredentials([azureServicePrincipal('acr-credentials-id-jenkins')]) {
                sh 'az login --service-principal -u $AZURE_CLIENT_ID -p $AZURE_CLIENT_SECRET -t $AZURE_TENANT_ID'
                
                dir('WeatherApps') {
                    echo "🚀 MNC Standard: Offloading build to Azure ACR Task..."
                    
                    // Ye line aapka Docker Plugin wala jhanjhat khatam kar degi
                    // Humein docker.withRegistry ki zaroorat nahi hai
                    sh "az acr build --registry acrlearn001 --image ${IMAGE_NAME}:${env.DOCKER_TAG} ."
                }
            }
        }

        stage('Deploy to DEV') {
            when { branch 'dev' }
            //--shared library p call ho raha hai ye function 
            steps { deployToVM("20.96.26.236", "8081", "dev") }
        }

        stage('Deploy to STAGING') {
            when { branch 'staging' }
            steps {
                input message: "Approve deployment to STAGING?", ok: "Deploy"
                //--shared library p call ho raha hai ye function
                deployToVM("23.x.x.x", "8081", "staging")
            }
        }

        stage('Deploy to PROD') {
            when { anyOf { branch 'main'; branch 'prod' } }
            steps {
                input message: "🚀 Ready for Production?", ok: "Deploy"
                //--shared library p call ho raha hai ye function
                deployToVM("52.x.x.x", "80", "prod")
            }
        }
    }

    // --- BEST PRACTICE: Post Actions ---
    post {
        success {
            echo "✅ Deployment Successful!"
            // Yahan aap Slack/Discord notification ka code daal sakte hain
        }
        failure {
            echo "🔴 Deployment Failed! Check logs immediately."
        }
        always {
            echo "🧹 Cleaning up workspace..."
            cleanWs() // Zaruri hai disk space ke liye
        }
    }
}



