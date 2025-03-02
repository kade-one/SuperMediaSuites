<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('stos', function (Blueprint $table) {
            $table->id();
            $table->date('date');
            $table->string('operator');
            $table->string('casting');
            $table->string('shift');
            $table->string('jam_sto');
            $table->string('fm');
            $table->json('data')->nullable();
            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('stos');
    }
};
