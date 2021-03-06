pipeline {
   agent any

   environment {
    
     // YOUR_DOCKERHUB_USERNAME (it doesn't matter if you don't have one)
     // get curretn commit sha, command 'git rev-parse HEAD' return full sha
     // if you wanna push image to dockerhub, image name must be unique
     GITCOMMITSHA = sh(returnStdout: true, script: "git log -n 1 --pretty=format:'%h'").trim()
     SERVICE_NAME = "chat-api"
      dockerhub_credential='DockerHubId'
      registry = 'registry.cn-hangzhou.aliyuncs.com/hyper/test'
     // registry = '839928622/chat-api'
     dockerImage = ''
   
   }

   stages {
      stage('Preparation') {
         steps {
            cleanWs()
            git credentialsId: 'GitHub', url: "https://github.com/${ORGANIZATION_NAME}/${SERVICE_NAME}"
         }
      }

      stage('Build Image') {
         steps {
             sh 'echo current git commit is ${GITCOMMITSHA}'
             sh 'docker image build  -t ${SERVICE_NAME}:latest -t ${SERVICE_NAME}:${GITCOMMITSHA} .'
           script {
              // documentation see: https://docs.cloudbees.com/docs/admin-resources/latest/plugins/docker-workflow#docker-workflow-sect-advanced
               dockerImage = docker.build registry
            }

         }
      }

        stage('push image and tag image') {
         steps {
           script {
            // this one is about to push to dockerhub
            // docker.withRegistry( '', dockerhub_credential){
            //    dockerImage.push("${GITCOMMITSHA}")
            //    dockerImage.push("latest")
            // }
             // and this one is about to push to private registry
             docker.withRegistry( 'https://registry.cn-hangzhou.aliyuncs.com', dockerhub_credential){
               dockerImage.push("${GITCOMMITSHA}")
               dockerImage.push("latest")
            }

           }
         }
      }

      stage('Deploy to Cluster') {
          steps {
                  sh 'kubectl apply -f deploy.yaml'
                  sh 'kubectl set image deployments/chat-api-experimental chat-api=${registry}:${GITCOMMITSHA}'
                   // after first deploy, u can comment this out
                  sh 'kubectl set image deployments/chat-api-original  chat-api=${registry}:ba25590'
                 
                 
                }
      }
   }
}
