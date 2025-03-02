# Production Management System Documentation

## 1. Project Overview

This is a Laravel 11-based production management system designed to handle manufacturing operations, including machine management, production tracking, and stock operations. The system provides comprehensive features for managing production data, generating reports, and monitoring manufacturing processes.

### Technology Stack
- Backend Framework: Laravel 11
- Database: MySQL
- Frontend: Blade templates with Tailwind CSS
- Authentication: Laravel built-in authentication

### Key Features
- User Authentication and Management
- Production Data Tracking (LHP - Laporan Hasil Produksi)
- Stock Take Operations (STO)
- Machine Management
- Product Model Management
- Multiple Export Formats
- Real-time Production Monitoring

## 2. Database Schema (ERD)

```mermaid
erDiagram
    USERS {
        id bigint PK
        name varchar
        email varchar
        email_verified_at timestamp
        password varchar
        remember_token varchar
        created_at timestamp
        updated_at timestamp
    }

    MACHINES {
        id bigint PK
        name varchar
        number integer
        created_at timestamp
        updated_at timestamp
    }

    PRODUCT_MODELS {
        id bigint PK
        name varchar
        is_active boolean
        created_at timestamp
        updated_at timestamp
    }

    PRODUCTION_DATA {
        id bigint PK
        machine_id bigint FK
        date date
        shift integer
        output_act integer
        tc integer
        st integer
        subtotal integer
        created_at timestamp
        updated_at timestamp
    }

    LHP_DETAILS {
        id bigint PK
        no_mesin varchar
        machine_name varchar
        created_at timestamp
        updated_at timestamp
    }

    STOS {
        id bigint PK
        date date
        operator varchar
        casting varchar
        shift varchar
        jam_sto varchar
        fm varchar
        data json
        created_at timestamp
        updated_at timestamp
    }

    MACHINES ||--o{ PRODUCTION_DATA : "has"
    USERS ||--o{ STOS : "creates"
    MACHINES ||--o{ LHP_DETAILS : "referenced_in"
```

## 3. System Architecture

### Directory Structure
```
laravel-11-mysql/
├── app/
│   ├── Exports/               # Export functionality
│   │   ├── Casting1Export.php
│   │   ├── Casting2Export.php
│   │   ├── CombinedExport.php
│   │   ├── CumulativeExport.php
│   │   └── StoExport.php
│   ├── Http/
│   │   └── Controllers/       # Application logic
│   │       ├── Auth/         # Authentication controllers
│   │       ├── LhpController.php
│   │       ├── PageController.php
│   │       ├── ProductModelController.php
│   │       ├── StoController.php
│   │       └── UserController.php
│   └── Models/               # Database models
│       ├── Machine.php
│       ├── ProductionData.php
│       ├── ProductModel.php
│       ├── Sto.php
│       └── User.php
├── resources/
│   └── views/               # Blade templates
│       ├── auth/           # Authentication views
│       ├── components/     # Reusable components
│       ├── layouts/       # Layout templates
│       ├── dashboard.blade.php
│       ├── lhp.blade.php
│       └── sto.blade.php
```

## 4. Process Flows

### Main User Flow
```mermaid
graph TD
    A[Start] --> B[Login Page]
    B --> C{Authentication}
    C -->|Success| D[Dashboard]
    C -->|Failure| B
    D --> E[Production Management]
    D --> F[STO Management]
    D --> G[Reports & Exports]
    E --> H[Machine Management]
    E --> I[Production Data Entry]
    F --> J[Stock Take Operations]
    G --> K[Generate Reports]
    K --> L[Export Data]
```

### Production Data Flow
```mermaid
graph TD
    A[Start] --> B[Select Machine]
    B --> C[Enter Production Data]
    C --> D[Input Validation]
    D -->|Valid| E[Save to Database]
    D -->|Invalid| C
    E --> F[Update Production Stats]
    F --> G[Generate LHP]
    G --> H[End]
```

### STO Process Flow
```mermaid
graph TD
    A[Start] --> B[Select Date & Shift]
    B --> C[Enter Operator Details]
    C --> D[Select Casting]
    D --> E[Input Stock Data]
    E --> F[Validation]
    F -->|Valid| G[Save STO Record]
    F -->|Invalid| E
    G --> H[Generate STO Report]
    H --> I[End]
```

## 5. Feature Details

### Authentication System
- Secure login/registration system
- Password reset functionality
- Email verification
- Remember me functionality
- Session management

### Production Management
- Machine registration and tracking
- Production data entry and monitoring
- Real-time production statistics
- LHP (Laporan Hasil Produksi) generation
- Historical data tracking

### Stock Take Operations (STO)
- Stock level monitoring
- Operator assignment
- Shift-wise stock tracking
- JSON-based flexible data storage
- Report generation

### Export Functionality
- Multiple export formats
- Casting-specific exports
- Combined data exports
- Cumulative reports
- STO reports

### User Interface
- Responsive design
- Intuitive navigation
- Data visualization
- Real-time updates
- Mobile-friendly layout

## 6. Security Considerations

### Authentication
- Secure password hashing
- CSRF protection
- Session management
- Rate limiting

### Data Protection
- Input validation
- SQL injection prevention
- XSS protection
- CORS policies

### Access Control
- Role-based access
- Route protection
- Resource authorization
- API authentication

## 7. Error Handling

### Types of Errors
- Authentication errors
- Validation errors
- Database errors
- System errors

### Error Management
- Detailed logging
- User-friendly error messages
- Error tracking
- Recovery procedures

## 8. Testing

### Test Types
- Unit tests
- Feature tests
- Integration tests
- Browser tests

### Testing Procedures
- Automated testing
- Manual testing
- Regression testing
- Performance testing
