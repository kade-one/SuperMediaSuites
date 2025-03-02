<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Login - STO Portal</title>
    <style>
        :root {
            --primary-color: #2c3e50;
            --accent-color: #3498db;
            --gradient-start: #2980b9;
            --gradient-end: #2c3e50;
            --error-color: #e74c3c;
            --success-color: #2ecc71;
            --text-color: #34495e;
            --light-text: #95a5a6;
            --background: #f5f6fa;
            --white: #ffffff;
            --shadow: rgba(0, 0, 0, 0.1);
            --transition: all 0.3s ease;
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        body {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }

        .login-container {
            width: 100%;
            max-width: 400px;
            background: var(--white);
            padding: 40px;
            border-radius: 20px;
            box-shadow: 0 15px 30px var(--shadow);
            transform: translateY(0);
            transition: var(--transition);
            position: relative;
            overflow: hidden;
        }

        .login-container:hover {
            transform: translateY(-5px);
            box-shadow: 0 20px 40px var(--shadow);
        }

        .login-container::before {
            content: '';
            position: absolute;
            top: 0;
            left: -100%;
            width: 100%;
            height: 3px;
            background: linear-gradient(90deg, transparent, var(--accent-color), transparent);
            animation: loading 2s linear infinite;
        }

        @keyframes loading {
            0% { left: -100%; }
            100% { left: 100%; }
        }

        h2 {
            color: var(--text-color);
            text-align: center;
            margin-bottom: 30px;
            font-size: 2em;
            font-weight: 600;
            position: relative;
            padding-bottom: 10px;
        }

        h2::after {
            content: '';
            position: absolute;
            bottom: 0;
            left: 50%;
            transform: translateX(-50%);
            width: 50px;
            height: 3px;
            background: linear-gradient(90deg, var(--gradient-start), var(--gradient-end));
            border-radius: 3px;
        }

        .error {
            background: rgba(231, 76, 60, 0.1);
            border-left: 4px solid var(--error-color);
            padding: 15px;
            margin-bottom: 20px;
            border-radius: 4px;
        }

        .error ul {
            list-style: none;
            color: var(--error-color);
            font-size: 0.9em;
        }

        .form-group {
            margin-bottom: 25px;
            position: relative;
        }

        .label-login {
            display: block;
            margin-bottom: 8px;
            color: var(--text-color);
            font-weight: 500;
            font-size: 0.9em;
            transition: var(--transition);
        }

        .form-control {
            width: 100%;
            padding: 12px 15px;
            border: 2px solid #e0e0e0;
            border-radius: 10px;
            font-size: 1em;
            transition: var(--transition);
            background: var(--background);
            color: var(--text-color);
        }

        .form-control:focus {
            border-color: var(--accent-color);
            outline: none;
            box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
        }

        .form-group:focus-within .label-login {
            color: var(--accent-color);
        }

        .btn {
            width: 100%;
            padding: 12px;
            border: none;
            border-radius: 10px;
            font-size: 1em;
            font-weight: 600;
            cursor: pointer;
            transition: var(--transition);
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .btn-primary {
            background:rgb(45, 71, 239);
            color: var(--white);
            box-shadow: 0 5px 15px rgba(41, 128, 185, 0.3);
        }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 20px rgba(41, 128, 185, 0.4);
        }

        .btn-primary:active {
            transform: translateY(0);
        }

        .links {
            margin-top: 25px;
            display: flex;
            justify-content: space-between;
            font-size: 0.9em;
        }

        .links a {
            color: var(--accent-color);
            text-decoration: none;
            transition: var(--transition);
            position: relative;
        }

        .links a::after {
            content: '';
            position: absolute;
            bottom: -2px;
            left: 0;
            width: 0;
            height: 2px;
            background: var(--accent-color);
            transition: var(--transition);
        }

        .links a:hover::after {
            width: 100%;
        }

        @media (max-width: 480px) {
            .login-container {
                padding: 30px 20px;
            }

            h2 {
                font-size: 1.7em;
            }

            .form-control {
                padding: 10px 12px;
            }

            .links {
                flex-direction: column;
                align-items: center;
                gap: 15px;
            }
        }

        /* Loading animation for button */
        .btn-loading {
            position: relative;
            pointer-events: none;
        }

        .btn-loading::after {
            content: '';
            position: absolute;
            width: 20px;
            height: 20px;
            top: 50%;
            left: 50%;
            margin: -10px 0 0 -10px;
            border: 3px solid rgba(255, 255, 255, 0.3);
            border-top-color: var(--white);
            border-radius: 50%;
            animation: rotate 0.8s linear infinite;
        }

        @keyframes rotate {
            to { transform: rotate(360deg); }
        }
    </style>
</head>
<body>
    <div class="login-container">
        <h2>STO LOGIN KYB</h2>
        @if ($errors->any())
            <div class="error">
                <ul>
                    @foreach ($errors->all() as $error)
                        <li>{{ $error }}</li>
                    @endforeach
                </ul>
            </div>
        @endif
        <form method="POST" action="{{ route('login') }}" id="loginForm">
            @csrf
            <div class="form-group">
                <label for="email" class="label-login">Email Address</label>
                <input type="email" id="email" name="email" class="form-control" required autocomplete="email" autofocus>
            </div>
            <div class="form-group">
                <label for="password" class="label-login">Password</label>
                <input type="password" id="password" name="password" class="form-control" required autocomplete="current-password">
            </div>
            <button type="submit" class="btn btn-primary" id="loginButton">LOGIN</button>
        </form>
    </div>

    <script>
        document.getElementById('loginForm').addEventListener('submit', function() {
            const button = document.getElementById('loginButton');
            button.classList.add('btn-loading');
            button.textContent = '';
        });

        // Add floating label effect
        document.querySelectorAll('.form-control').forEach(input => {
            input.addEventListener('focus', () => {
                input.parentElement.classList.add('focused');
            });
            
            input.addEventListener('blur', () => {
                if (!input.value) {
                    input.parentElement.classList.remove('focused');
                }
            });
        });
    </script>
</body>
</html>