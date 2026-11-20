# Deploying to Azure Functions (Flex Consumption)

`.github/workflows/deploy-azure-functions.yml` deploys the API on every new `vX.Y.Z` tag.

The API is a regular ASP.NET Core app, not a Functions-model project, so it runs as a
[custom handler](https://learn.microsoft.com/azure/azure-functions/functions-custom-handlers):
the Functions host starts the self-contained `OCID.Api` binary on `FUNCTIONS_CUSTOMHANDLER_PORT`
and forwards every HTTP request to it. The files in this folder are added to the publish output:

| File | Role |
| --- | --- |
| `host.json` | Starts `OCID.Api --urls http://127.0.0.1:<port>`, removes the `/api` route prefix, forwards raw HTTP requests |
| `api/function.json` | One catch-all anonymous HTTP trigger (`{*route}`); authentication stays in the app (JWT) |

## One-time Azure setup

Replace the `<...>` values.

```bash
# 1. Function app on Flex Consumption (run `az functionapp list-runtimes` to confirm the custom runtime name)
az functionapp create --resource-group <RG> --name <APP_NAME> --storage-account <STORAGE> \
  --flexconsumption-location <REGION> --runtime custom --runtime-version 1.0

# 2. App settings (the app reads them like environment variables)
az functionapp config appsettings set --resource-group <RG> --name <APP_NAME> --settings \
  "ConnectionStrings__DefaultConnection=<sql connection string>" \
  "Jwt__Key=<32+ chars secret>" \
  "NLOG_LOG_DIR=/tmp/logs"

# 3. Identity GitHub Actions logs in with (OIDC, no secret stored in GitHub)
az identity create --name ocid-github-deploy --resource-group <RG>
az role assignment create --assignee <identity principalId> --role "Website Contributor" \
  --scope $(az functionapp show --name <APP_NAME> --resource-group <RG> --query id -o tsv)
az identity federated-credential create --identity-name ocid-github-deploy --resource-group <RG> \
  --name github-production \
  --issuer https://token.actions.githubusercontent.com \
  --subject repo:DiiageCUCDB/DI2-P2-P3-E2-OCID-API:environment:production \
  --audiences api://AzureADTokenExchange
```

## One-time GitHub setup

1. **Settings > Environments**: create an environment named `production` (optionally add required reviewers).
2. **Settings > Secrets and variables > Actions**:
   - secrets: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`
   - variable: `AZURE_FUNCTIONAPP_NAME`

## Things to know

- **Tags created by `semantic-release` with the default `GITHUB_TOKEN` do not trigger other workflows.**
  `docker.yml` creates the release tags that way, so for this workflow to fire automatically,
  give `semantic-release` a personal access token or GitHub App token instead of `GITHUB_TOKEN`.
  Otherwise run this workflow manually (Actions > Deploy to Azure Functions > Run workflow, choosing the tag).
- The custom handler must start listening within **60 seconds**. `Program.cs` applies EF Core
  migrations before the app starts serving, so a long migration can exceed that on a cold start.
- Docs endpoints (`/scalar`, `/openapi`) are disabled in `Production`, which is the default environment here.
