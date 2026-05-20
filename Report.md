# Group B: Carls ⏰
**DevOps, Software Evolution and Software Maintenance** *IT University of Copenhagen*

| Name | Email |
| :--- | :--- |
| Bror Yang Nan Hansen | `broh@itu.dk` |
| Carl | `csti@itu.dk` |
| Carl | `cfth@itu.dk` |
| Konrad | `koad@itu.dk` |
| Mikkel | `mikcl@itu.dk` |

---

## Table of Contents
- [Group B: Carls ⏰](#group-b-carls-)
  - [Table of Contents](#table-of-contents)
  - [1. System's Perspective](#1-systems-perspective)
  - [2. Process' Perspective](#2-process-perspective)
  - [3. Reflection Perspective](#3-reflection-perspective)
    - [3.1 Group Coordination and Task Management (Evolution \& Refactoring)](#31-group-coordination-and-task-management-evolution--refactoring)
    - [3.2 Database Migration and Syntax Clashes (Operation)](#32-database-migration-and-syntax-clashes-operation)
    - [3.3 Trivy Vulnerability Scans and Exception Management (Maintenance \& CI/CD)](#33-trivy-vulnerability-scans-and-exception-management-maintenance--cicd)
    - [Reflecting on the DevOps style of work](#reflecting-on-the-devops-style-of-work)
  - [While our DevOps tech and implementation was successful our team dynamic was a bit of a mess. The idea of DevOps is to share the responsibility, but the way the tasks was set up incentivized us to limit the number of people working at a time in order to effectively refactor the architecture in time. The big takeaway here is that doing DevOps right takes a real commitment to cross-training and pair programming. Next time around, we need to make sure everyone gets to learn and deploy and refactor the system rather than a few experts doing most of the work.](#while-our-devops-tech-and-implementation-was-successful-our-team-dynamic-was-a-bit-of-a-mess-the-idea-of-devops-is-to-share-the-responsibility-but-the-way-the-tasks-was-set-up-incentivized-us-to-limit-the-number-of-people-working-at-a-time-in-order-to-effectively-refactor-the-architecture-in-time-the-big-takeaway-here-is-that-doing-devops-right-takes-a-real-commitment-to-cross-training-and-pair-programming-next-time-around-we-need-to-make-sure-everyone-gets-to-learn-and-deploy-and-refactor-the-system-rather-than-a-few-experts-doing-most-of-the-work)
  - [Use of Generative AI](#use-of-generative-ai)
  - [References](#references)
  - [Appendix](#appendix)

---

## 1. System's Perspective
A description and illustration of the:

Design and architecture of your ITU-MiniTwit systems.
All dependencies of your ITU-MiniTwit systems on all levels of abstraction and development stages. That is, list and briefly describe all technologies and tools you applied and depend on.
Describe the current state of your systems, for example using results of static analysis and quality assessments.


---

## 2. Process' Perspective
This perspective should clarify how code or other artifacts come from idea into the running system and everything that happens on the way.

In particular, the following descriptions should be included:

A complete description and illustration of stages and tools included in the CI/CD pipelines, including deployment and release of your systems.
How do you monitor your systems and what precisely do you monitor?
What do you log in your systems and how do you aggregate logs?
Brief description of how you security hardened your systems.
How do you handle availability and scaling in your systems?


---

## 3. Reflection Perspective
### 3.1 Group Coordination and Task Management (Evolution & Refactoring)
During the refactoring phases of the system, the primary challenge was falling behind. The structure of the tasks were often interdependent on each other making it hard to delegate tasks out in a way where pair programming could be taken advantage of. An attempt at mob programming with a rotating driver and navigator was made in the beginning but was unsuccessful, as some people would lose focus just listening. Our solution was to limit active development to a maximum of 2-3 team members concurrently while the remaining members would focus on asynchronous tasks like doing the exercises to understand the eventual pull request they would review later. 



### 3.2 Database Migration and Syntax Clashes (Operation)
The primary technical challenge was migrating our database from SQLite to PostgreSQL while trying to achieve as little downtime in our system as possible. To achieve this a tool called `pgloader` was used which makes it possible to load data from a SQLite database directly into a postgres database. Initially the migration went very well. A new DigitalOcean droplet and a clean postgres database was created. `pgloader` commands were then used to migrate the data itself and it all worked out. That was until a couple days later when the status overview dashboard graphs displayed a sudden surge in failed message requests as well as user registration requests. The culprit was the different type safety the two databases use.

SQLite uses very loose and forgiving data types, whereas PostgreSQL is very strict. This results in mismatches in the database where PostgreSQL expects a certain type which wasn't translated by pgloader. A good example is the Cheep.Timestamp: 

`public required DateTime TimeStamp { get; set; }`

In the SQLite database the timestamps were stored as Unix integers, and when pgloader migrated the data to PostgreSQL, it kept it as an integer column. However the Entity Framework Core looks at the Cheep model and sees a strict `DateTime` type. 

Another example is SQL Dialect Clashes. For new data insertions, our EF Core configuration was hardcoded to use `.HasDefaultValueSql("datetime('now')")`. While valid in SQLite, PostgreSQL does not recognize this specific function, causing all new `INSERT` operations (registrations and messages) to instantly crash at the database level.

The solution to these issues was creating AI-assisted SQL `ALTER` scripts to correct the type mismatches, successfully casting the integers to proper PostgreSQL timestamps. And for the second issue the solution can be seen here: [Pull Request #34](https://github.com/DevOps-Group-B/MiniTwit/pull/34).


### 3.3 Trivy Vulnerability Scans and Exception Management (Maintenance & CI/CD)
Another major issue were false positive. Trivy works by scanning all dependencies that are used in the project. If any of the libraries used have been found to have a vulnerability the CI/CD pipeline will fail. Even when updating the dependencies to a newer version that Trivy suggested, the issue reoccurred. The problem was realizing that Trivy stops the pipeline due to *transitive* dependency vulnerabilities existing deeper within third-party libraries that are used by the direct dependencies we used.

In some cases, no matter what version we updated our dependencies to, it would still be flagged, essentially resulting in false positive. The solution to this was to make a `.trivyignore` file that would ignore the vulnerabilities that other libraries have in order for the CI/CD pipeline to continue.
 
 
We learned this was an issue that we could not do anything about due to complex library dependency chains. Something we have no control over. Pull Request [#75](https://github.com/DevOps-Group-B/MiniTwit/pull/75), [#76](https://github.com/DevOps-Group-B/MiniTwit/pull/76), [#82](https://github.com/DevOps-Group-B/MiniTwit/pull/82), [#88](https://github.com/DevOps-Group-B/MiniTwit/pull/88), and [#130](https://github.com/DevOps-Group-B/MiniTwit/pull/130) are examples of vulnerabilities being added to the `.trivyignore` list.


### Reflecting on the DevOps style of work
**Technical Reflection**
In previous projects, the workflow was highly traditional. Groups would use feature branch Git workflows where they write code locally and push it to a shared repository. Live deployments were rarely needed, but the classical issue of "why doesn't it work on my machine" would often occur. The DevOps style of work has taught us the methods to avoid these issues like relying on containerization of apps using `Docker`. Getting better at using `Github Actions` for continuous integration and deployment(CI/CD) has also been a huge help when working with these bigger DevOps projects. Once the pipeline was established, code would be tested and deployed automatically, drastically reducing manual overhead. Learning about code linters, Static Code Analysis and SonarQube has also been very helpful to avoid smaller unnoticeable mistakes and in general writing better code.

Learning `Vagrant`, and later refactor to use `Terraform` for Infrastructure as Code(IaC) has also been important to ensure our system and architecture is easily reproducible, scalable and free from manual configuration drift.

Lastly working with real servers has been very fun. Using DigitalOcean as our server provider has been very easy and efficient. The next step to full ownership and more privacy is to deploy on a private server like a raspberry pi or an old laptop.

**Work Ethics**
While our DevOps tech and implementation was successful our team dynamic was a bit of a mess. The idea of DevOps is to share the responsibility, but the way the tasks was set up incentivized us to limit the number of people working at a time in order to effectively refactor the architecture in time. The big takeaway here is that doing DevOps right takes a real commitment to cross-training and pair programming. Next time around, we need to make sure everyone gets to learn and deploy and refactor the system rather than a few experts doing most of the work. 
---

## Use of Generative AI

ITU's rules on the use of generative AI apply for this report too. They are described here and in detail here. Please follow them. For your report that means that you have to state which generative AI tools have been used for which task(s) in your projects. Additionally, describe how generative AI tools have been used and briefly reflect and discuss how they supported or hindered your work process.

---

## References


---

## Appendix
*(Attach supplemental materials here)*