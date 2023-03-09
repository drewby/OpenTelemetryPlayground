# Container Labels for Obervability

In order to tag observability artifacts, we add build labels as environment 
variables to the docker container. The environment variables include:

* **BUILDTIME** - the date/time stamp of the current build
* **VERSION** - a version number
* **REVISION** - the git commit hash that triggered the build
* **APPNAME** - the name of the application

These environment variables can then be included in obervability data such as logs.
A common usage is the REVISION or commit hash, which can be shortened to the first
seven characters and still be useable to identify the running version of the app
when included in logs, traces, metrics, etc.

Example:
```
[16:49:23 f8b854c ERR] An unhandled exception has occurred while executing the request.
```

## Implementation

The build labels are available from the GitHub workflow and passed on to the 
Docker build step as build-args.

```yaml
build-args: |
    BUILDTIME=${{ fromJSON(steps.meta.outputs.json).labels['org.opencontainers.image.created'] }}
    VERSION=${{ fromJSON(steps.meta.outputs.json).labels['org.opencontainers.image.version'] }}
    REVISION=${{ fromJSON(steps.meta.outputs.json).labels['org.opencontainers.image.revision'] }}
```

In the Dockerfile, we create environment variables using the build arguments.

```Dockerfile
ARG BUILDTIME
ARG VERSION
ARG REVISION=*NOREV*
ARG APPNAME=WebApi

ENV BUILDTIME $BUILDTIME
ENV VERSION $VERSION
ENV REVISION $REVISION
ENV APPNAME $APPNAME
```
We also echo these variables to the console when anyone attaches to the container by including them in the .bashrc file.

```Dockerfile
RUN echo "echo -e \"Application\t: \$APPNAME\"" >> /root/.bashrc
RUN echo "echo -e \"Build time\t: \$BUILDTIME\"" >> /root/.bashrc
RUN echo "echo -e \"Version\t\t: \$VERSION\"" >> /root/.bashrc
RUN echo "echo -e \"Revision\t: \$REVISION\"" >> /root/.bashrc
```

Example:
```bash
Application     : stocks-sample/webapi
Build time      : 2022-06-28T08:08:43.912Z
Version         : 2022.06.28.16
Revision        : f8b854ce0eb6bf4d3682f2193c0e0b1ba760fa3f
root@webapi-deployment-76fffbb979-n84mp:/app#
```