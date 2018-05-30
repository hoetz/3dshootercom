FROM microsoft/dotnet:2.1-sdk as builder
WORKDIR /app
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish --output /out/ --configuration Release

FROM microsoft/dotnet:2.1-aspnetcore-runtime

RUN apt-get update \
	&& apt-get install -y --no-install-recommends openssh-server \
	&& echo "root:Docker!" | chpasswd
COPY --from=builder /app/sshd_config /etc/ssh/
COPY --from=builder /app/init_container.sh .
WORKDIR /myapp/
COPY --from=builder /out .
EXPOSE 2222 80
RUN chmod 755 /init_container.sh
ENTRYPOINT ["/init_container.sh"]