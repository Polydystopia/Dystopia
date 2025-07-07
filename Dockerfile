FROM mcr.microsoft.com/dotnet/runtime-deps:6.0
WORKDIR /dystopia

COPY ./dystopia_release .

EXPOSE 5000

ENTRYPOINT ["/dystopia/Dystopia"]
