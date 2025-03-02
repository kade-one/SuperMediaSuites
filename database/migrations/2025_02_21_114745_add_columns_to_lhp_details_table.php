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
        Schema::table('lhp_details', function (Blueprint $table) {
            $table->string('machine_name')->after('no_mesin')->nullable();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::table('lhp_details', function (Blueprint $table) {
            $table->dropColumn('machine_name');
        });
    }
};
