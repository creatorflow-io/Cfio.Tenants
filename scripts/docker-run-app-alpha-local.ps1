# $env: is specific to PowerShell
docker image tag cfiotenantsapp:dev ductrantb/cfiotenantsapp:dev 
docker stop TenantApp
docker rm TenantApp
docker run --name TenantApp --env ASPNETCORE_URLS="http://+:80;https://+:443" --env=ASPNETCORE_ENVIRONMENT=Development --env ASPNETCORE_Kestrel__Certificates__Default__Path=/home/app/.aspnet/https/aspnetapp.pfx --env ASPNETCORE_Kestrel__Certificates__Default__Password=devcerts --env PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin --env DOTNET_RUNNING_IN_CONTAINER=true --env DOTNET_VERSION=6.0.26 --env=ASPNET_VERSION=6.0.26 --volume=$env:AppData\ASP.NET\Https\aspnetapp.crt:/etc/ssl/certs/aspnetapp.crt:ro --volume=$env:AppData\ASP.NET\Https:/home/app/.aspnet/https:ro  --volume=D:\UserSecrets:/root/.microsoft/usersecrets:ro --volume=D:\UserSecrets:/home/app/.microsoft/usersecrets:ro --workdir=/app -p 44314:443 -p 8014:80 --runtime=runc -d ductrantb/cfiotenantsapp:dev

