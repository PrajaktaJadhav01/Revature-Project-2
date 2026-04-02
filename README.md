# Customer Management System

A complete Customer Relationship Management (CRM) platform built with a microservices architecture. This system provides a robust backend to handle customer data and an interactive React frontend for a seamless user experience.

## 🏗️ Architecture Stack

### Backend
- **Framework**: .NET (C#)
- **Architecture**: Microservices Pattern
- **Key Components**:
  - `ApiGateway`: Single point of entry for all client requests
  - `CustomerApi`: Microservice dedicated to managing customer data
- **Design Patterns**: CQRS (Command Query Responsibility Segregation)
- **Service Discovery**: HashiCorp Consul (for reliable microservice-to-microservice communication)

### Frontend
- **Framework**: React 19 with Vite
- **Language**: TypeScript
- **Styling**: Clean, custom CSS for a modern, responsive design
- **State Management / Routing**: React Router DOM
- **Data Fetching**: Axios
- **Icons**: Lucide React

## 🚀 Key Features

*   **Customer Management**: View, search, and manage a directory of customers seamlessly.
*   **Modern Interactive Dashboard**: Custom-built UI features like `CustomerModal`, `CustomerCard`, and a dynamic `NavBar`.
*   **Scalable Backend**: Designed to handle scalable requests using API Gateway and individual robust sub-services.
*   **Service Discovery**: Integrating Consul for healthy service registration and load distribution.

## 🛠️ Getting Started

### Prerequisites

*   [.NET SDK](https://dotnet.microsoft.com/download)
*   [Node.js](https://nodejs.org/)
*   [Consul](https://developer.hashicorp.com/consul/downloads) (Must be running for service discovery)

### Running the Backend

1.  Make sure your local Consul agent is running:
    ```bash
    consul agent -dev
    ```
2.  Open the solution file `CustomerManagement.sln` in Visual Studio or navigate to the project roots via terminal.
3.  Start the **ApiGateway** and **CustomerApi** projects.
    ```bash
    cd CustomerApi
    dotnet run
    ```
    ```bash
    cd ApiGateway
    dotnet run
    ```

### Running the Frontend

1.  Navigate to the `frontend` directory:
    ```bash
    cd frontend
    ```
2.  Install dependencies:
    ```bash
    npm install
    ```
3.  Start the development server:
    ```bash
    npm run dev
    ```
4.  Open your browser and navigate to the local server (usually `http://localhost:5173`).

## 📁 Project Structure

*   `ApiGateway/`: The API entry point. routes external requests to internal microservices.
*   `CustomerApi/`: The primary backend service responsible for CRUD operations on users/customers.
*   `CQRS/`: Contains Command and Query handlers to adhere to CQRS principles for the services.
*   `ConsulDemo/`: Implementation details/demos regarding Consul integration.
*   `frontend/`: The React + Vite application. Contains components (`CustomerCard`, `CustomerModal`, `NavBar`), and integration logics (`api.ts`).
