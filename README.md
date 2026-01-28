# Aydin Wylde Portfolio

A modern, Matrix-themed professional portfolio website built with ASP.NET Core 9.0, featuring an integrated blog system, admin panel, and visitor analytics.

![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4)
![License](https://img.shields.io/badge/license-Proprietary-red)
![DevExtreme](https://img.shields.io/badge/DevExtreme-24.1-FF7200)

⚠️ **PROPRIETARY SOFTWARE** - This repository is for demonstration purposes only. All rights reserved. See [LICENSE.txt](LICENSE.txt) for details.

## 🌟 Features

### 🎨 **Matrix-Themed Design**

- Animated Matrix digital rain background
- Custom mouse glow effects and particle trails
- Smooth parallax scrolling sections
- 3D card tilt effects on hover
- Professional loading screen with progress animation

### 🔐 **Admin Panel**

- Secure authentication system
- Clean, modern dashboard interface
- Quick access via **Alt + Shift + A** hotkey
- Real-time visitor analytics
- Comprehensive blog post management

### 📝 **Blog Management System**

- Full CRUD operations for blog posts
- Category organization
- Featured posts spotlight
- Tag-based filtering
- Rich HTML content support
- Author attribution
- Publish date tracking

### 📊 **Analytics Dashboard**

- Real-time visitor tracking
- Daily, weekly, and monthly statistics
- Page view analytics
- Browser and device type tracking
- Visitor journey monitoring

### 🎯 **Portfolio Sections**

- **Home**: Hero section with quick stats
- **About**: Professional introduction
- **Skills**: Categorized technical expertise
  - Backend Development
  - Frontend Development
  - Cloud & DevOps
  - Architecture
  - Databases
  - Mobile Development
  - Certifications
  - Tools
- **Projects**: Portfolio showcase
- **Education**: Academic background
- **Blog**: Technical articles and insights
- **Contact**: Get in touch section

## 🛠️ Technologies

### Backend

- **ASP.NET Core 9.0** - Web framework
- **C# 12** - Programming language
- **XML Storage** - Data persistence

### Frontend

- **DevExtreme 24.1** - UI components
- **jQuery 3.7.1** - DOM manipulation
- **Font Awesome 6.1.1** - Icons
- **Custom CSS3** - Styling and animations
- **Vanilla JavaScript** - Interactive features

### Security

- Session-based authentication
- Credential encryption
- Protected admin routes
- CORS configuration

## 📋 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Windows, macOS, or Linux
- Modern web browser (Chrome, Edge, Firefox, Safari)

## 🚀 Installation

**Note**: This software is proprietary. The instructions below are for demonstration purposes only.

1. **View the repository**
   ```bash
   # Repository is read-only and for viewing only
   git clone https://github.com/ajdin-ahmagic-dotnet/AydinWyldePortfolioX.git
   cd AydinWyldePortfolioX
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - HTTP: `http://localhost:5001`
   - HTTPS: `https://localhost:7001`

## 🔧 Configuration

### Port Configuration

Default ports are configured in `Properties/launchSettings.json`:
- **HTTP**: 5001
- **HTTPS**: 7001

### Admin Default Credentials

On first run, use the following default credentials:
- **Username**: `admin`
- **Password**: `admin123`

⚠️ **Important**: Change the default password immediately after first login!

### Data Storage

The application uses XML files stored in the `App_Data` folder:
- `App_Data/Secure/admin_credentials.xml` - Admin authentication
- `App_Data/Blog/blog_posts.xml` - Blog content
- `App_Data/Analytics/visitor_stats.xml` - Analytics data

## 📖 Usage

### Accessing the Admin Panel

1. **Via Keyboard Shortcut** (Recommended)
   - Press `Alt + Shift + A` anywhere on the site

### Admin Panel Features

#### Analytics Tab

- View visitor statistics (today, this week, this month, all-time)
- Monitor recent activity
- Track popular pages

#### Blog Posts Tab

- Create new blog posts
- Edit existing content
- Delete posts
- Toggle featured status
- Manage categories and tags

#### Settings Tab

- Change admin password
- Configure site settings

### Creating a Blog Post

1. Open admin panel (`Alt + Shift + A`)
2. Navigate to "Blog Posts" tab
3. Click "New Post" button
4. Fill in the form:
   - Title (required)
   - Category
   - Summary
   - Content (HTML supported)
   - Tags (comma-separated)
   - Featured checkbox
5. Click "Save Post"

## 📁 Project Structure

```
AydinWyldePortfolioX/
├── Controllers/
│   ├── HomeController.cs          # Main application routes
│   └── AdminController.cs         # Admin panel API endpoints
├── Models/
│   ├── BlogPost.cs               # Blog post data model
│   ├── AdminCredentials.cs       # Admin auth model
│   └── VisitorTracking.cs        # Analytics model
├── Services/
│   ├── BlogService.cs            # Blog operations
│   ├── AdminService.cs           # Admin operations
│   ├── VisitorTrackingService.cs # Analytics service
│   └── NotificationService.cs    # Notifications
├── Middleware/
│   └── VisitorTrackingMiddleware.cs # Analytics tracking
├── Views/
│   ├── Home/                     # Page views
│   └── Shared/
│       └── _Layout.cshtml        # Main layout
├── wwwroot/
│   ├── css/
│   │   ├── Site.css             # Main styles
│   │   ├── Site2.css            # Additional styles
│   │   └── devextreme/          # DevExtreme themes
│   ├── js/
│   │   └── devextreme/          # DevExtreme libraries
│   └── fonts/                    # Monoid font
├── App_Data/                     # XML data storage
├── Program.cs                    # Application entry point
└── appsettings.json             # Configuration
```

## 📝 License

**Copyright © 2026 Aydin Wylde. All Rights Reserved.**

This project is proprietary software. Unauthorized copying, modification, distribution, or use of this software, via any medium, is strictly prohibited without express written permission from the copyright holder.

See [LICENSE.txt](LICENSE.txt) for full license terms.

## 👤 Author

**Aydin Wylde**
- GitHub: [@ajdin-ahmagic-dotnet](https://github.com/ajdin-ahmagic-dotnet)
- Repository: https://github.com/ajdin-ahmagic-dotnet/AydinWyldePortfolioX

## 🙏 Acknowledgments

- [DevExtreme](https://js.devexpress.com/) - UI component library
- [Font Awesome](https://fontawesome.com/) - Icon library
- [Monoid Font](https://larsenwork.com/monoid/) - Programming font
- Matrix digital rain inspiration from the Wachowskis' *The Matrix*

## 📞 Contact

For licensing inquiries or questions:
- Open an issue on GitHub
- Contact via the portfolio website

---

**Built with ❤️ using ASP.NET Core 9.0**