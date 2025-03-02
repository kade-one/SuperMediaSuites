<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\PageController;
use App\Http\Controllers\StoController;
use App\Http\Controllers\AuthController;
use App\Http\Controllers\UserController;
use App\Http\Controllers\LhpController;
use App\Http\Controllers\ProductModelController; // Tambahkan import ini

// ... rest of the routes code stays the same ...


Route::get('/', function () {
    return redirect()->route('login');
});

Route::get('/plan-gm', function () {
    return view('plan-gm');
})->middleware('auth');


Route::get('/mtn-core', function () {
    return view('mtn-core');
})->middleware('auth');

Route::get('/sto-display', function () {
    return view('sto_display');
})->middleware('auth');

Route::get('/page', [PageController::class, 'index'])->middleware('auth');
Route::get('/sto', [PageController::class, 'sto'])->middleware('auth');
Route::post('/sto', [StoController::class, 'store'])->middleware('auth');
Route::get('/sto/display', [StoController::class, 'show'])->middleware('auth');

Route::get('login', [AuthController::class, 'showLoginForm'])->name('login');
Route::post('login', [AuthController::class, 'login']);

Route::get('register', [AuthController::class, 'showRegisterForm'])->name('register');
Route::post('register', [AuthController::class, 'register']);
Route::get('users', [UserController::class, 'index'])->name('users.index')->middleware('auth');

Route::middleware(['auth'])->group(function () {
    // Dashboard route
    Route::get('dashboard', function () {
        return view('dashboard');
    })->name('dashboard');

    // STO routes
    Route::get('/sto', [PageController::class, 'sto']);
    Route::post('/sto', [StoController::class, 'store']);
    Route::get('/sto/display', [StoController::class, 'show'])->name('sto.display');
    Route::get('/sto/latest', [StoController::class, 'index'])->name('sto.latest');
    Route::get('/sto-display', [StoController::class, 'index']);
});

Route::get('password/reset', [AuthController::class, 'showLinkRequestForm'])->name('password.request');
Route::post('password/email', [AuthController::class, 'sendResetLinkEmail'])->name('password.email');
Route::post('logout', [AuthController::class, 'logout'])->name('logout')->middleware('auth');


Route::get('/lhp', [LhpController::class, 'create'])->name('lhp.create');
Route::post('/lhp', [LhpController::class, 'store'])->name('lhp.store');
Route::get('/lhp-display', [LhpController::class, 'display'])->name('lhp.display');


// Model Management Routes
Route::middleware(['auth'])->group(function () {
    Route::get('/models', [ProductModelController::class, 'index'])->name('models.index');
// Model Management Routes
Route::middleware(['auth'])->group(function () {
    Route::get('/models', [ProductModelController::class, 'index'])->name('models.index');
    Route::post('/models', [ProductModelController::class, 'store'])->name('model.store');
});
});


Route::get('/sto/export', [StoController::class, 'export'])->name('sto.export');