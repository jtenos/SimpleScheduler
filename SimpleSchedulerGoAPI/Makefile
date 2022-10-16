## tidy: format code and tidy modfile
.PHONY: tidy
tidy:
	go fmt ./...
	go mod tidy -v

## build: build the cmd/api application
.PHONY: build
build:
	go mod verify
	go build -ldflags='-s' -o=./bin/api ./cmd/api
	GOOS=linux GOARCH=amd64 go build -ldflags='-s' -o=./bin/linux_amd64/api ./cmd/api
	cp ./cmd/api/conf.json ./bin/conf.json

## run: run the cmd/api application
.PHONY: run
run: tidy build
	./bin/api
