 docker build -f .\src\Cfio.Tenants.App\Dockerfile --tag cfiotenantsapp:dev --rm --build-arg GITHUB_PACKAGE_USERNAME --build-arg GITHUB_PACKAGE_TOKEN .