## cert: generate a key for the JWT
.PHONY: cert
cert:
	openssl genrsa -out cert/id_rsa 4096
	openssl rsa -in cert/id_rsa -pubout -out cert/id_rsa.pub

## tidy: format code and tidy modfile
.PHONY: tidy
tidy:
	go fmt ./...
	go mod tidy -v

## build: build the cmd/api application
.PHONY: build
build: tidy
	go mod verify
	go build -ldflags='-s' -o=./bin/api ./cmd/api
	GOOS=linux GOARCH=amd64 go build -ldflags='-s' -o=./bin/linux_amd64/api ./cmd/api
	cp ./cmd/api/conf.json ./bin/linux_amd64/conf.json

## run: run the cmd/api application
.PHONY: run
run: build
	./bin/linux_amd64/api
