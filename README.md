# Eshava.DomainDrivenDesign
A library that provides a basic implementation of the domain driven design pattern

## Introduction
This library is intended to serve as a guide to the application of the domain-driven design pattern. 
For this purpose, basic implementations in the form of abstract classes are available for each layer.
The goal is to standardize code and create a clearly defined project structure.

The abstract classes of the infrastructure layer are designed for use with SQL-based databases. Access to other types of databases is not currently implemented.
The use of Entity Framework (EF) for database access is not planned. In my opinion, the use of EF only makes sense if the models used are encapsulated within the infrastructure layer. These models must not leave the infrastructure layer.

A simple sample API is provided for better understanding of the approach. The infrastructure project also includes an SQL script for creating the database tables.


## Domain-Project
The project forms the core of the application. It contains only domain models, value objects, and C# enums. These are organized at the top level by domain and then either by feature or model name.
There are three types of domain models: standalone models, aggregate models, and child models. The latter are contained in aggregate models. 
Due to nesting, a child model can also be a aggregate model at the same time.
All models have in common that they must be valid in themselves in order to be saved. 
This means that each model implements a set of validation rules that monitor the state of the model. 
Aggregate models can also contain validation rules that refer to their child models. For example, if two child models are not allowed to have the same name.
Validation rules that require information from outside the domain model are not permitted.

Value objects represent components of the domain model. These cannot be changed after creation and have a parameterized constructor. 
The information contained in a value object is treated as a unit. If any information contained in the value object is changed, the entire value object must be recreated.
Value objects are used as properties of the domain models. Nested value objects are not supported.

The lifecycle of a model, from creation to modification to deactivation, can be relevant for the own or other domains.
For this reason, every model action generates a model-internal event. The standard events are "created", "changed" and "deactivated". Additional events can be added.
The automatic generation of these events can be deactivated in each model and is activated by default. For an aggregate model, there are additional settings for creating events related to its child models.
The events are distributed when saved (see Infrastructure Project).

* Domain-Project
	* Domain-Name
		* Feature-Name
			* DomainModel1 
			* DomainModel2
			* ValueObject1
			* C#-Enum1
		* Model3-Name
			* DomainModel3
			* C#-Enum2

			
## Application-Project
This project contains all of the application's business logic that cannot or may not be implemented within a domain model. The application project only has access to the domain project.
This business logic is organized into use cases, which can be roughly divided into read actions (queries) and write actions (commands).
No direct access to external resources is permitted within this project. External resources include databases, APIs, file access, etc. 
This access must be encapsulated via the infrastructure project. Communication takes place via so-called InfrastructureProviderServices. These are also divided into read (query) and write (command) operations.

The structure within the project is similar to the domain project. At the top level, the structure is organized according to domain, followed by features and models or directly by models without features.
This is followed by a distinction between queries and commands. At this level, the interfaces of the InfrastructureProviderServices belonging to the model are stored. 
General InfrastructureProviderServices without model reference, e.g., for API access, are stored at higher levels or in separate domains.

Below the query and command levels are the folders for use cases, e.g., Read or Create. Everything required for the use case is stored within these folders.
In its minimal form, an use case always consists of the use case itself, its interfaces, a request, and a response.
All validation rules that represent a connection between the domain model and the outside world must be applied within the use cases. For example, references to another domain model.

* Application-Project
	* Domain-Name
		* Feature-Name 
			* Model1-Name
				* Commands
					* IDomainModel1InfrastructureProviderService
					* Create
						* DomainModel1CreateDto
						* DomainModel1CreateValueObjectDto
						* DomainModel1CreateRequest
						* DomainModel1CreateResponse
						* DomainModel1CreateUseCase
						* IDomainModel1CreateUseCase
					* Update
						* ...	
				* Queries
					* IModel1QueryInfrastructureProviderService
					* Read
						* Model1ReadDto
						* Model1ReadRequest
						* Model1ReadResponse
						* Model1ReadUseCase
						* IModel1ReadUseCase
		* Nodel3-Name
			* Commands
				* ... 
			* Queries
				* ... 
	* Settings
		* ScopedSettings
		* AppSettings
		* ...

		
## Infrastructure-Project
This project encapsulates all access to external resources. When saving and loading domain models, it is responsible for maintaining the integrity of the domain model. 
This means that domain models are always transferred to or released from this project as a unit.
The infrastructure project only has access to the application project and the domain project.
The structure within this project is based on the domains and models. Model-independent resources are stored in separate domains or at a higher level wherever possible.
The InfrastructureProviderService form the only interface to the application project. These encapsulate access to resource brokers such as model repositories, API wrappers, file access, etc. 
These are also divided into read (query) and write (command) operations.
When saving and loading aggregate models, the InfrastructureProviderService orchestrates the individual resource accesses. 
If, for example, a model consists of several individual models, the InfrastructureProviderService distributes the storage of the individual models to the respective repositories.
In the current implementation, the information of a domain model and the associated value objects are mapped to the same data model.
In principle, however, the information of a value object could also be stored in a separate table with reference to the main data model. 
The InfrastructureProviderServices has another task. It collects and distributes the events contained in the domain models. To do this, the domain model events are first converted into domain events. 

* Infrastructure-Project
	* Domain-Name
		* Model1-Name
			* DataModel1
			* DataModel1DbConfiguration
			* DomainModel1InfrastructureProviderService
			* DomainModel1Repository
			* IDomainModel1Repository
			* IModel1QueryRepository
			* Model1QueryInfrastructureProviderService
			* Model1QueryRepository
		* Model2-Name
			* ...
		* DomainNameTransformationProfile 
	

## Api-Project
This project represents the interface to the outside world. All use cases are called via this interface. 
Instead of an API project, a UI can also be connected directly. In the case of an API project, only endpoints are provided.
The API project only has access to the application project.

* Api-Project
	* Endpoints
		* Model1Endpoints
			* GET Read
			* POST Create
			* ...
		* Model2Endpoints
		* ...
	* Program


## Project Dependencies

Implicit Access Level
``` 
-------------------------------------------------------------------------
| Api                                                                   |
-------------------------------------------------------------------------
     ||               ||              ||
     ||               ||              \/
     ||               ||         ----------------------------------------
     ||               ||         | Infrastructure                       |
     ||               ||         ----------------------------------------
     ||               ||              ||                    ||
     ||               \/              \/                    ||
     ||          -------------------------------------      ||
     ||          | Application                       |      ||
     ||          -------------------------------------      ||
     ||                        ||                           ||
     \/                        \/                           \/
-------------------------------------------------------------------------
| Domain                                                                |
-------------------------------------------------------------------------
```

Explicit Access Level
``` 
-------------------------------------------------------------------------
| Api                                                                   |
-------------------------------------------------------------------------
                      ||              
                      ||              
                      ||         ----------------------------------------
                      ||         | Infrastructure                       |
                      ||         ----------------------------------------
                      ||              ||                    ||
                      \/              \/                    ||
                 -------------------------------------      ||
                 | Application                       |      ||
                 -------------------------------------      ||
                               ||                           ||         
                               \/                           \/         
-------------------------------------------------------------------------
| Domain                                                                |
-------------------------------------------------------------------------
```