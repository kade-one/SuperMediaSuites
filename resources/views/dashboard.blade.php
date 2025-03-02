@extends('layouts.app')
@section('content')

<div class="container">
    <h1 class="welcome-title">Selamat Datang di Dashboard</h1>
    
    <div class="grid">
        <a href="/sto" class="card">
            <div class="card-content sto">
                <div class="card-header">
                    <div class="icon-wrapper">
                        <i class="fas fa-chart-bar card-icon"></i>
                    </div>
                    <div class="card-text">
                        <h2 class="card-title">STO</h2>
                        <p class="card-description">Manajemen dan monitoring data STO</p>
                    </div>
                </div>
            </div>
        </a>

        <a href="/sto-display" class="card">
            <div class="card-content sto-display">
                <div class="card-header">
                    <div class="icon-wrapper">
                        <i class="fas fa-eye card-icon"></i>
                    </div>
                    <div class="card-text">
                        <h2 class="card-title">STO DISPLAY</h2>
                        <p class="card-description">Tampilan visual dan laporan STO</p>
                    </div>
                </div>
            </div>
        </a>

        <a href="/lhp" class="card">
            <div class="card-content lhp">
                <div class="card-header">
                    <div class="icon-wrapper">
                        <i class="fas fa-file-alt card-icon"></i>
                    </div>
                    <div class="card-text">
                        <h2 class="card-title">LHP</h2>
                        <p class="card-description">Manajemen Laporan Hasil Pekerjaan</p>
                    </div>
                </div>
            </div>
        </a>

        <a href="/lhp-display" class="card">
            <div class="card-content lhp-display">
                <div class="card-header">
                    <div class="icon-wrapper">
                        <i class="fas fa-chart-pie card-icon"></i>
                    </div>
                    <div class="card-text">
                        <h2 class="card-title">LHP DISPLAY</h2>
                        <p class="card-description">Tampilan visual laporan LHP</p>
                    </div>
                </div>
            </div>
        </a>
    </div>
</div>

<style>
    * {
        margin: 0;
        padding: 0;
        box-sizing: border-box;
        font-family: 'Arial', sans-serif;
    }

    body {
        min-height: 100vh;
        background: linear-gradient(to bottom right, #f8f9fa, #e9ecef);
    }

    /* Main Content Styles */
    .container {
        max-width: 1200px;
        margin: 0 auto;
        padding: 2rem;
    }

    .welcome-title {
        text-align: center;
        font-size: 2rem;
        color: #1f2937;
        margin-bottom: 2rem;
    }

    .grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
        gap: 1.5rem;
    }

    .card {
        text-decoration: none;
        color: inherit;
    }

    .card-content {
        background: white;
        border-radius: 12px;
        padding: 1.5rem;
        transition: transform 0.3s, box-shadow 0.3s;
        height: 100%;
    }

    .card-content:hover {
        transform: translateY(-5px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }

    .card-header {
        display: flex;
        align-items: flex-start;
        gap: 1rem;
    }

    .icon-wrapper {
        padding: 1rem;
        background: #f8f9fa;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.05);
        flex-shrink: 0;
    }

    .card-icon {
        font-size: 1.5rem;
    }

    .card-text {
        flex: 1;
    }

    .sto .card-icon { color: #2563eb; }
    .sto-display .card-icon { color: #7c3aed; }
    .lhp .card-icon { color: #059669; }
    .lhp-display .card-icon { color: #dc2626; }

    .card-title {
        font-size: 1.25rem;
        color: #1f2937;
        margin-bottom: 0.25rem;
    }

    .card-description {
        color: #6b7280;
        font-size: 0.875rem;
        line-height: 1.4;
    }

    /* Responsive Design */
    @media (max-width: 1024px) {
        .grid {
            grid-template-columns: repeat(2, 1fr);
        }
    }

    @media (max-width: 768px) {
        .container {
            padding: 1.5rem;
        }

        .welcome-title {
            font-size: 1.75rem;
        }

        .grid {
            grid-template-columns: 1fr;
            gap: 1.25rem;
        }
    }

    @media (max-width: 480px) {
        .container {
            padding: 1rem;
        }

        .welcome-title {
            font-size: 1.5rem;
            margin-bottom: 1.5rem;
        }

        .card-content {
            padding: 1.25rem;
        }

        .icon-wrapper {
            padding: 0.875rem;
        }

        .card-icon {
            font-size: 1.25rem;
        }

        .card-title {
            font-size: 1.125rem;
        }
    }
</style>
@endsection