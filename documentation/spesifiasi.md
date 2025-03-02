# Technical Specification Document

## 1. API Endpoints

### Authentication Endpoints
```
POST   /login                 - User authentication
POST   /register             - New user registration
POST   /password/email       - Send password reset link
POST   /password/reset       - Reset password
POST   /logout              - User logout
```

### Production Management Endpoints
```
# Machines
GET    /machines             - List all machines
POST   /machines             - Create new machine
GET    /machines/{id}        - Get machine details
PUT    /machines/{id}        - Update machine
DELETE /machines/{id}        - Delete machine

# Production Data
GET    /production           - List production data
POST   /production           - Create production record
GET    /production/{id}      - Get production details
PUT    /production/{id}      - Update production record
DELETE /production/{id}      - Delete production record

# LHP (Laporan Hasil Produksi)
GET    /lhp                  - View LHP list
POST   /lhp                  - Create new LHP
GET    /lhp/{id}            - View specific LHP
PUT    /lhp/{id}            - Update LHP
DELETE /lhp/{id}            - Delete LHP
```

### Stock Take Operation (STO) Endpoints
```
GET    /sto                  - List STO records
POST   /sto                  - Create STO record
GET    /sto/{id}            - Get STO details
PUT    /sto/{id}            - Update STO record
DELETE /sto/{id}            - Delete STO record
```

### Export Endpoints
```
GET    /export/casting1      - Export Casting 1 data
GET    /export/casting2      - Export Casting 2 data
GET    /export/combined      - Export combined data
GET    /export/cumulative    - Export cumulative report
GET    /export/sto          - Export STO data
```

## 2. Database Models

### User Model
```php
class User extends Authenticatable
{
    protected $fillable = [
        'name',
        'email',
        'password',
    ];

    protected $hidden = [
        'password',
        'remember_token',
    ];

    protected $casts = [
        'email_verified_at' => 'datetime',
        'password' => 'hashed',
    ];
}
```

### Machine Model
```php
class Machine extends Model
{
    protected $fillable = [
        'name',
        'number'
    ];

    public function productionData()
    {
        return $this->hasMany(ProductionData::class);
    }
}
```

### ProductionData Model
```php
class ProductionData extends Model
{
    protected $fillable = [
        'machine_id',
        'date',
        'shift',
        'output_act',
        'tc',
        'st',
        'subtotal'
    ];

    protected $casts = [
        'date' => 'date'
    ];

    public function machine()
    {
        return $this->belongsTo(Machine::class);
    }
}
```

### Sto Model
```php
class Sto extends Model
{
    protected $fillable = [
        'date',
        'operator',
        'casting',
        'shift',
        'jam_sto',
        'fm',
        'data'
    ];

    protected $casts = [
        'date' => 'date',
        'data' => 'json'
    ];
}
```

## 3. Export Classes

### Base Export Structure
```php
namespace App\Exports;

use Maatwebsite\Excel\Concerns\FromCollection;
use Maatwebsite\Excel\Concerns\WithHeadings;

class BaseExport implements FromCollection, WithHeadings
{
    protected $data;
    protected $headings;

    public function collection()
    {
        return $this->data;
    }

    public function headings(): array
    {
        return $this->headings;
    }
}
```

### Specific Export Types
- Casting1Export: Exports data specific to Casting 1
- Casting2Export: Exports data specific to Casting 2
- CombinedExport: Combines data from multiple sources
- CumulativeExport: Generates cumulative reports
- StoExport: Exports STO-specific data

## 4. Frontend Components

### Layout Structure
```blade
<!-- layouts/app.blade.php -->
<!DOCTYPE html>
<html>
<head>
    <title>@yield('title')</title>
    <link href="{{ asset('css/app.css') }}" rel="stylesheet">
    <link href="{{ asset('css/base-table.css') }}" rel="stylesheet">
</head>
<body>
    @include('components.navbar')
    <main>
        @yield('content')
    </main>
</body>
</html>
```

### Key Components
1. Navbar Component
```blade
<!-- components/navbar.blade.php -->
<nav>
    <div class="logo">
        <img src="{{ asset('images/kyb.png') }}" alt="Logo">
    </div>
    <div class="menu">
        <!-- Navigation links -->
    </div>
</nav>
```

2. STO Table Component
```blade
<!-- components/sto-table.blade.php -->
<table class="sto-table">
    <thead>
        <!-- Table headers -->
    </thead>
    <tbody>
        <!-- Dynamic STO data -->
    </tbody>
</table>
```

## 5. JavaScript Functionality

### STO Management
```javascript
// public/js/sto.js
document.addEventListener('DOMContentLoaded', function() {
    // STO form handling
    // Data validation
    // AJAX submissions
    // Dynamic updates
});
```

## 6. Security Implementation

### Authentication Middleware
```php
protected $routeMiddleware = [
    'auth' => \App\Http\Middleware\Authenticate::class,
    'auth.basic' => \Illuminate\Auth\Middleware\AuthenticateWithBasicAuth::class,
    'auth.session' => \Illuminate\Session\Middleware\AuthenticateSession::class,
    'cache.headers' => \Illuminate\Http\Middleware\SetCacheHeaders::class,
    'can' => \Illuminate\Auth\Middleware\Authorize::class,
    'password.confirm' => \Illuminate\Auth\Middleware\RequirePassword::class,
    'verified' => \Illuminate\Auth\Middleware\EnsureEmailIsVerified::class,
];
```

### CSRF Protection
```php
protected $middleware = [
    \App\Http\Middleware\VerifyCsrfToken::class,
];
```

## 7. Error Handling

### Exception Handler
```php
class Handler extends ExceptionHandler
{
    protected $dontReport = [
        AuthenticationException::class,
        AuthorizationException::class,
        ValidationException::class,
    ];

    public function register(): void
    {
        $this->reportable(function (Throwable $e) {
            // Custom error reporting
        });
    }
}
```

## 8. Testing Configuration

### PHPUnit Configuration
```xml
<?xml version="1.0" encoding="UTF-8"?>
<phpunit xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:noNamespaceSchemaLocation="vendor/phpunit/phpunit/phpunit.xsd"
         bootstrap="vendor/autoload.php"
         colors="true">
    <testsuites>
        <testsuite name="Unit">
            <directory>tests/Unit</directory>
        </testsuite>
        <testsuite name="Feature">
            <directory>tests/Feature</directory>
        </testsuite>
    </testsuites>
</phpunit>
```

## 9. Deployment Requirements

### Server Requirements
- PHP >= 8.2
- MySQL >= 8.0
- Composer
- Node.js & NPM
- SSL Certificate

### Environment Variables
```env
APP_NAME=ProductionManagement
APP_ENV=production
APP_DEBUG=false
APP_URL=https://your-domain.com

DB_CONNECTION=mysql
DB_HOST=127.0.0.1
DB_PORT=3306
DB_DATABASE=your_database
DB_USERNAME=your_username
DB_PASSWORD=your_password
```

### Deployment Steps
1. Clone repository
2. Install dependencies
3. Set environment variables
4. Run migrations
5. Compile assets
6. Configure web server
7. Set up SSL
8. Configure cron jobs
9. Set up queue worker
