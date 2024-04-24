import json
import pulumi
import pulumi_aws as aws
import pulumi_awsx as awsx

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
    managed_policy_arns=[aws.iam.ManagedPolicy.AWS_LAMBDA_EXECUTE],
)

repo = awsx.ecr.Repository("repo", force_delete=True)
image = awsx.ecr.Image(
    "image",
    awsx.ecr.ImageArgs(
        repository_url=repo.url,
        context="../app/",
        platform="linux/amd64",
    ),
)

fn = aws.lambda_.Function(
    "fn",
    package_type="Image",
    role=role.arn,
    timeout=500,
    image_uri=image.image_uri,
)

api = aws.lambda_.FunctionUrl(
    "fn_url", function_name=fn.name, authorization_type="NONE"
)

pulumi.export("ECR repository URL", repo.url)
pulumi.export("Lambda URL", api.function_url)
