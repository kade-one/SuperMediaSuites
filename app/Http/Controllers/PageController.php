<?php

// app/Http/Controllers/PageController.php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use App\Models\ProductModel;

class PageController extends Controller
{
    public function index()
    {
        return view('page');
    }
    public function sto()
    {
        $models = ProductModel::where('is_active', true)
                              ->orderBy('name')
                              ->pluck('name');
        return view('sto', compact('models'));
    }
}