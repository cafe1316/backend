# Cafe1316 low-cost AWS deployment

Production architecture:

```text
GitHub dev branch
  |-- frontend workflow -> AWS Amplify
  `-- backend workflow  -> Amazon ECR -> AWS Lambda -> API Gateway HTTP API
                                                    -> Render PostgreSQL
```

This design has no always-on AWS compute. Lambda and API Gateway charge for use,
while the existing Render PostgreSQL plan remains the only always-on database.

## 1. Branch workflow

The remote default branch of both repositories is `dev`.

1. Develop on a feature branch.
2. Push the feature branch and open a pull request targeting `dev`.
3. The pull request runs build, lint, and tests without deploying.
4. Merge into `dev` only after CI passes.
5. A push to `dev` deploys the production AWS resources.

In GitHub, protect `dev` and require the CI checks before merging. In the
`production` environment, allow deployment only from `dev`. The environment
restriction is important because the AWS OIDC role trusts the GitHub
`production` environment.

## 2. Choose one AWS Region

Use the same Region for ECR, Lambda, API Gateway, SSM, and CloudFormation. For an
Australia-focused portfolio, `ap-southeast-2` is a reasonable default. If the
Render PostgreSQL database is in another geography, compare latency before making
the final choice.

Every following example assumes:

```bash
AWS_REGION=ap-southeast-2
AWS_ACCOUNT_ID=123456789012
```

Replace the example account ID with the real value. Do not paste secrets into
the shell history.

## 3. Create the ECR repository

Create a CloudFormation stack named `cafe1316-ecr` using `infra/aws/ecr.yml`.
The stack creates a private repository with vulnerability scanning and lifecycle
rules that delete untagged images and retain only six release images.

Record these outputs:

- `RepositoryName`
- `RepositoryArn`
- `RepositoryUri`

## 4. Create the four SSM SecureString parameters

In Systems Manager Parameter Store, create Standard tier `SecureString`
parameters under this exact hierarchy:

```text
/cafe1316/production/DATABASE_URL
/cafe1316/production/JwtSettings/Secret
/cafe1316/production/Stripe/SecretKey
/cafe1316/production/Stripe/WebhookSecret
```

The application loads `/cafe1316/production` at Lambda startup. The configuration
provider removes the prefix and maps the remaining slash-separated name to an
ASP.NET Core configuration key. For example:

```text
/cafe1316/production/JwtSettings/Secret -> JwtSettings:Secret
```

Use the existing Render external PostgreSQL URL for `DATABASE_URL`. Parameter
Store Standard parameters do not add a monthly secret-storage charge. The Lambda
execution role can read only parameters below this application path.

## 5. Push the initial Lambda image

CloudFormation needs an existing image to create a container-based function, so
the first image is a one-time bootstrap step:

```bash
aws ecr get-login-password --region "$AWS_REGION" |
  docker login --username AWS --password-stdin \
  "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"

docker build --platform linux/amd64 -t cafe1316-api:bootstrap .

docker tag cafe1316-api:bootstrap \
  "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/cafe1316-api:bootstrap"

docker push \
  "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/cafe1316-api:bootstrap"
```

The image uses the AWS-provided .NET 10 Lambda base image. Its command is the
assembly name, `Cafe1316.API`, because `AddAWSLambdaHosting` provides the Lambda
event handler.

## 6. Create GitHub's AWS OIDC provider once

In IAM, create an OpenID Connect provider:

- Provider URL: `https://token.actions.githubusercontent.com`
- Audience: `sts.amazonaws.com`

Record the provider ARN. GitHub Actions will use short-lived AWS credentials;
do not create or save long-lived AWS access keys in GitHub.

## 7. Create Lambda and API Gateway

Create a CloudFormation stack named `cafe1316-serverless-api` using
`infra/aws/lambda-api.yml`. Supply:

- `ImageUri`: the full `:bootstrap` ECR image URI
- `EcrRepositoryArn`: output from the ECR stack
- `GitHubOidcProviderArn`: the account-level OIDC provider ARN
- `FrontendOrigin`: exact Amplify URL without a trailing slash
- `GoogleClientId`: Google web client ID
- `StripePublishableKey`: Stripe publishable key

The stack creates:

- a 512 MB Lambda container function with a 30-second timeout;
- an API Gateway HTTP API using payload format 2.0;
- a `$default` route that forwards all paths and methods to ASP.NET Core;
- seven-day CloudWatch log retention;
- a least-privilege runtime role for logs and SSM;
- a GitHub OIDC deployment role scoped to this repository and environment.

Record these outputs:

- `FunctionName`
- `ApiUrl`
- `GitHubDeployRoleArn`

## 8. Configure GitHub backend deployment

In `cafe1316/backend`, create a GitHub environment named `production`. Restrict
its deployment branches to `dev`.

Add repository or environment variables:

```text
AWS_REGION=ap-southeast-2
ECR_REPOSITORY=cafe1316-api
LAMBDA_FUNCTION_NAME=<FunctionName output>
API_GATEWAY_URL=<ApiUrl output>
```

Add one environment secret:

```text
AWS_DEPLOY_ROLE_ARN=<GitHubDeployRoleArn output>
```

On every push to `dev`, `.github/workflows/aws-deploy.yml` will test the solution,
push `github.sha` and `latest` image tags, update Lambda to the immutable SHA tag,
wait for the update, and call `/health` through API Gateway.

## 9. Configure Amplify

Connect `cafe1316/frontend` to Amplify Hosting and choose `dev` as the production
branch. Amplify automatically reads `amplify.yml`.

Set this Amplify environment variable:

```text
VITE_API_BASE_URL=<ApiUrl output>/api
```

After Amplify gives the final production URL, update `FrontendOrigin` in the
Lambda CloudFormation stack if it differs from the value used initially. This
updates both API Gateway and ASP.NET Core CORS.

If the frontend GitHub workflow should explicitly start Amplify builds, create
an Amplify incoming webhook for `dev` and store its URL in the frontend GitHub
`production` environment as `AMPLIFY_WEBHOOK_URL`. Otherwise, let Amplify's native
repository integration deploy `dev` and remove the workflow's deploy job.

## 10. Database migrations

Lambda must not run EF Core migrations during startup because multiple cold
starts can occur concurrently. The production template therefore sets:

```text
DatabaseInitialization__ApplyMigrations=false
DatabaseInitialization__Seed=false
```

When a release contains a new EF migration, run it once as an explicit release
step from a trusted machine or a dedicated migration workflow before updating the
Lambda image:

```bash
dotnet ef database update \
  --project Cafe1316.Infrastructure \
  --startup-project Cafe1316.API
```

Use a temporary environment variable or .NET user-secrets for `DATABASE_URL`;
never commit it.

## 11. Verification and rollback

Verify:

```bash
curl --fail "<ApiUrl>/health"
```

Then test login, products, cart, checkout, Stripe webhook handling, and orders
from the Amplify URL.

Each deployed image is tagged with its Git commit SHA. To roll back, update the
Lambda function code to a previously known-good ECR SHA tag. Do not use `latest`
as the rollback identifier.

## Cost controls

- No provisioned concurrency: Lambda can scale to zero.
- HTTP API instead of REST API: lower request pricing for this use case.
- 512 MB Lambda memory: a conservative starting point for ASP.NET Core cold starts.
- Seven-day logs: prevents indefinite CloudWatch storage growth.
- ECR lifecycle: only six release images are retained.
- SSM Standard parameters: no monthly per-secret storage charge.
- Render PostgreSQL remains in place: no duplicate RDS fixed cost.

Set an AWS Budget alert before deploying. Data transfer, unusually high traffic,
long-running requests, or accidental retry loops can still produce charges.
