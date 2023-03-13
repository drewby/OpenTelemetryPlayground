FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /app

# Copy project
COPY ./*.csproj ./
# Restore as distinct layers
RUN dotnet restore

# Copy everything else
COPY . ./

# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
ARG BUILDTIME
ARG VERSION
ARG REVISION

RUN apt-get update; apt-get install curl -y

WORKDIR /app
COPY --from=build-env /app/out .

ENV BUILDTIME $BUILDTIME
ENV VERSION $VERSION
ENV REVISION $REVISION

RUN echo "echo -e \"Application\t: dsp-api/webapi\"" >> /root/.bashrc
RUN echo "echo -e \"Build time\t: \$BUILDTIME\"" >> /root/.bashrc
RUN echo "echo -e \"Version\t\t: \$VERSION\"" >> /root/.bashrc
RUN echo "echo -e \"Revision\t: \$REVISION\"" >> /root/.bashrc

ENV ASPNETCORE_URLS "http://*:5000"
ENTRYPOINT ["dotnet", "todoapi.dll"]