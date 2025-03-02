<?php

namespace App\Http\Controllers;

use App\Models\ProductModel;
use Illuminate\Http\Request;

class ProductModelController extends Controller
{
    public function index()
    {
        $models = ProductModel::orderBy('name')->get();
        return view('models.create', compact('models'));
    }

    public function store(Request $request)
    {
        $request->validate([
            'name' => 'required|string|max:255|unique:product_models,name'
        ]);

        $model = new ProductModel();
        $model->name = $request->name;
        $model->is_active = true;
        $model->save();

        return redirect()->route('models.index')->with('success', 'Model berhasil ditambahkan');
    }
}
