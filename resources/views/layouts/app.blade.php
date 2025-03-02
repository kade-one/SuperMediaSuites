<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>KSTS KAYABA</title>
  <link rel="stylesheet" href="{{ asset('css/app.css') }}">
  <link rel="stylesheet" href="{{ asset('css/base-table.css') }}">
  
  <style>
    html {
      zoom: 100%;
    }
  </style>  
</head>
<body>
  <button class="toggle-button" onclick="toggleSidebar()">☰</button>
  @include('components.navbar')
  <div class="content" style="margin-top: 60px; padding: 15px;">
    @yield('content')
  </div>



  <script>
    function toggleSidebar() {
      document.querySelector('.navbar').classList.toggle('active');
    }
  </script>
</body>
</html>