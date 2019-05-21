pipeline {
    agent {
        kubernetes {
            label 'hot-potato'
            yamlFile './build-spec.yml'
        }
    }

    stages {
        stage('Version') {
            environment {
                IGNORE_NORMALISATION_GIT_HEAD_MOVE = "1"
            }
            steps {
                container('gitversion') {
                    script {
                        env.IMAGE_VERSION = sh(script: 'mono /usr/lib/GitVersion/GitVersion.exe /output json /showvariable NuGetVersionV2', returnStdout: true).trim()
                        echo IMAGE_VERSION 
                    }
                }
            }
        }

        stage("Build") {
            steps {
                container("builder") {
                    sh 'dotnet build --configuration Release -p:Version=${IMAGE_VERSION}'
                }
            }
        }

        stage("Run-Unit-Tests") {
            steps {
                container("builder") {
                    //sh 'dotnet test ./test/HotPotato.Core.Test/HotPotato.Core.Test.csproj -c Release -l:"trx;LogFileName=$WORKSPACE/test/results/CoreResults.xml" -r $WORKSPACE/test/results --no-restore --no-build'
                    sh 'dotnet test ./test/HotPotato.Core.Test/HotPotato.Core.Test.csproj -c Release -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura -p:CoverletOutput=./test/results/cobertura-coverage.xml --no-restore --no-build'

                    //sh 'dotnet test ./test/HotPotato.AspNetCore.Middleware.Test/HotPotato.AspNetCore.Middleware.Test.csproj --configuration Release -r middleware-test-results.xml --no-restore --no-build'
                   // sh 'dotnet test ./test/HotPotato.OpenApi.Test/HotPotato.OpenApi.Test.csproj --configuration Release -r openapi-test-results.xml --no-restore --no-build'
                }
            }
        }
		
		// stage("Run-Integration-Tests") {
        //     steps {
        //         container("builder") {
        //             sh 'dotnet test ./test/HotPotato.Integration.Test/HotPotato.Integration.Test.csproj --configuration Release -r integration-test-results.xml --no-restore --no-build'
        //         }
        //     }
        // }

        // stage("Run-E2E-Tests") {
        //     steps {
        //         container("builder") {
        //             sh 'dotnet test ./test/HotPotato.E2E.Test/HotPotato.E2E.Test.csproj --configuration Release -r E2E-test-results.xml --no-restore --no-build'
        //         }
        //     }
        // }
    }
    post {
        always {
            cobertura coberturaReportFile: '**/test/results/cobertura-coverage.xml'
        }
        regression {
            mattermostSend color: "#ef1717", icon: "https://jenkins.io/images/logos/jenkins/jenkins.png", message: "Someone broke ${env.BRANCH_NAME}, Ref build number -- ${env.BUILD_NUMBER}! (<${env.BUILD_URL}|${env.BUILD_URL}>)"
        }

        fixed {
            mattermostSend color: "#7FFF00", icon: "https://jenkins.io/images/logos/jenkins/jenkins.png", message: "Someone fixed ${env.BRANCH_NAME}, Ref build number -- ${env.BUILD_NUMBER}! (<${env.BUILD_URL}|${env.BUILD_URL}>)"
        }
    }
}