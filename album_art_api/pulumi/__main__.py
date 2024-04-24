import json
import pulumi
import pulumi_aws as aws
import pulumi_aws_apigateway as apigateway
import pulumi_docker as docker

role = aws.iam.Role(
    "role",
    assume_role_policy=json.dumps(
        {
            "Version": "2012-10-17",
            "Statement": [
                {
                    "Action": "sts:AssumeRole",
                    "Effect": "Allow",
                    "Principal": {
                        "Service": "lambda.amazonaws.com",
                    },
                }
            ],
        }
    ),
    managed_policy_arns=[aws.iam.ManagedPolicy.AWS_LAMBDA_BASIC_EXECUTION_ROLE],
)

image = docker.Image(
    "app-image",
    # build=docker.DockerBuild(context="app/Dockerfile"),
    image_name="ghcr.io/dhzdhd/w11-mediaplayerrpc:latest",
    registry=docker.ImageRegistry(
        server="ghcr.io",
        username=pulumi.Config("github").require("dhzdhd"),
    ),
    skip_push=False,
)

fn = aws.lambda_.Function(
    "fn",
    timeout=300,
    role=role.arn,
    package_type="Image",
    image_uri=image.base_image_name,
)

api = apigateway.RestAPI(
    "api",
    routes=[
        apigateway.RouteArgs(path="/", method=apigateway.Method.GET, event_handler=fn),
    ],
)

pulumi.export("url", api.url)
