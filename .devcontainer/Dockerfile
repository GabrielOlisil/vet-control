FROM mcr.microsoft.com/dotnet/sdk:10.0

COPY --from=node:24-bookworm-slim /usr/local/bin/node /usr/local/bin/node
COPY --from=node:24-bookworm-slim /usr/local/include/node /usr/local/include/node
COPY --from=node:24-bookworm-slim /usr/local/lib/node_modules /usr/local/lib/node_modules

RUN ln -s /usr/local/lib/node_modules/npm/bin/npm-cli.js /usr/local/bin/npm \
    && ln -s /usr/local/lib/node_modules/npm/bin/npx-cli.js /usr/local/bin/npx


USER ubuntu


RUN dotnet tool install --global dotnet-ef

ENV PATH="$PATH:/home/ubuntu/.dotnet/tools"

