<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

class CreateProductionDataTable extends Migration
{
    public function up()
{
    Schema::create('production_data', function (Blueprint $table) {
        $table->id();
        $table->foreignId('machine_id')->constrained('machines')->onDelete('cascade');
        $table->date('date');
        $table->integer('shift');
        $table->integer('output_act')->default(0);
        $table->integer('tc')->default(0);
        $table->integer('st')->default(0);
        $table->integer('subtotal')->default(0);
        $table->timestamps();
    });
}

    public function down()
    {
        Schema::dropIfExists('production_data');
    }
}