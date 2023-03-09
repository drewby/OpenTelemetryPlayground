buildtime=$(shell date --iso-8601=seconds)
revision=$(shell date +%H%M)
version=1.0.0

build-ops:
	dotnet publish src/ops -c Release -o lib/Ops

define build_component
@rm -rf $(CURDIR)/src/$(1)/lib
@cp -r $(CURDIR)/lib $(CURDIR)/src/$(1)/lib
@cd src/$(1); docker build -t $(1):latest . \
		 --build-arg REVISION=$(revision) \
         --build-arg BUILDTIME=$(buildtime) \
         --build-arg VERSION=$(version) 
@docker tag $(1):latest localhost:5555/$(1):latest
@docker push localhost:5555/$(1):latest
endef

# TODO: there should be a way to parameterix build-todoapi and build-backend
build-todoapi: build-ops
	$(call build_component,todoapi)

build-todospa: 
	@cd src/todospa; \
	npm install; \
	npx parcel build index.html --no-cache --public-url ./ --dist-dir ../../deploy/web
	kubectl rollout restart deployment/todospa -n apps

# build-backend: build-ops
# 	$(call build_component,backend)

define deploy_component
kubectl apply -k $(CURDIR)/deploy/kustomize/apps/$(1)
kubectl rollout restart deployment/$(1) -n apps
endef

deploy-todoapi: 
	$(call deploy_component,todoapi)

# deploy-backend: 
# 	$(call deploy_component,backend)

all-todoapi: build-todoapi deploy-todoapi

# all-backend: build-backend deploy-backend

all: all-todoapi build-todospa