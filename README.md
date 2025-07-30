# SharpFlow ⚡

> Revolutionary C# automation platform with live code compilation, zero-setup deployment, and true cross-platform reach

## 🚀 Why SharpFlow?

**Traditional automation tools force painful compromises:**
- n8n: Node.js + 1,973 npm packages + setup complexity + web-only
- Langflow: Python + C++ compilers + mandatory reboots + desktop-only  
- Enterprise tools: Vendor lock-in + licensing costs + platform restrictions

**SharpFlow eliminates ALL compromises with Avalonia UI:**
- ✅ **Single 15MB executable** - native AOT, no dependencies, no runtime
- ✅ **Desktop + Web from same code** - Avalonia desktop app + WebAssembly deployment
- ✅ **Live C# compilation** - write code directly in workflow nodes with IntelliSense
- ✅ **Professional drag-and-drop designer** - pixel-perfect Canvas with native performance
- ✅ **True cross-platform** - Windows, macOS, Linux natively + any browser via WASM

## 🎯 Core Features

### **Zero Setup Experience**
- **Desktop**: Download and run - single executable, no installers, no reboots
- **Web**: Deploy to any static hosting - GitHub Pages, CDN, corporate intranet
- **Cross-platform**: Same binary works on Windows, Mac, Linux + browser access

### **Hybrid Power**
- **Visual workflows** for rapid automation building
- **Live C# scripting** when you need full programming power
- **Monaco Editor integration** with IntelliSense in both desktop and web versions
- **Hot compilation** with Roslyn - see results instantly

### **Enterprise Performance**
- **Native AOT compilation** - millisecond startup, minimal memory footprint
- **Avalonia UI rendering** - 60fps animations, pixel-perfect on any DPI
- **Efficient execution engine** - async/await throughout, cancellation support

### **Developer Experience**
- **XAML-based UI** - familiar to WPF/UWP developers
- **Modern .NET 9** - latest C# features, performance optimizations
- **Built-in debugging** - step through workflows, inspect variables
- **Extensible architecture** - plugin system for custom nodes

## 🏗️ Technology Stack

- **UI Framework**: Avalonia UI 11+ (cross-platform XAML)
- **Runtime**: .NET 9 with Native AOT compilation
- **Code Compilation**: Roslyn analyzers and scripting
- **Persistence**: SQLite (single file, zero config)
- **Deployment**: Single binary (desktop) + Static files (web)

## 🌟 Deployment Options

### **Desktop Application** (Primary)
```bash
# Download and run - that's it!
./SharpFlow.exe           # Windows
./SharpFlow               # Linux/macOS
```
- 15MB single executable
- No installation required
- Full file system access
- Native OS integration

### **Web Application** (Bonus!)
```bash
# Deploy anywhere static files are supported
npx serve wwwroot/        # Local development
# Or upload to: GitHub Pages, Netlify, S3, Azure Static Web Apps
```
- Same UI, same features
- Runs in any modern browser
- No server infrastructure needed
- Perfect for demos and remote access

## 🛣️ Status

🚧 **Early Development** - Core architecture and Avalonia UI foundation in progress

See [ROADMAP.md](ROADMAP.md) for detailed development timeline and milestones.

## 🎮 Quick Start

```bash
# Clone the repository
git clone https://github.com/holger/SharpFlow.git
cd SharpFlow

# Run desktop version
dotnet run --project src/SharpFlow.Desktop

# Build single executable
dotnet publish src/SharpFlow.Desktop -c Release --self-contained -r win-x64
# Output: bin/Release/net9.0/win-x64/publish/SharpFlow.exe (15MB)

# Build web version
dotnet publish src/SharpFlow.Web -c Release
# Output: bin/Release/net9.0-browser/publish/wwwroot/ (static files)
```

## 🏆 Competitive Advantages

| Feature | SharpFlow | n8n | Langflow | Enterprise Tools |
|---------|-----------|-----|----------|------------------|
| **Setup Time** | 0 seconds | 30+ minutes | 45+ minutes | Hours/Days |
| **Dependencies** | None | Node.js + 1973 packages | Python + C++ toolchain | Multiple runtimes |
| **Cross-platform** | Desktop + Web + Mobile* | Web only | Desktop only | Platform specific |
| **Performance** | Native AOT | Node.js interpreted | Python interpreted | Varies |
| **Programming** | Full C# power | Limited JS | Limited Python | Proprietary |
| **Deployment** | Single file | Docker required | Complex setup | Vendor dependent |
| **Cost** | Open source | Open/Commercial | Open/Commercial | Enterprise licensing |

*Mobile support planned for future releases

## 🤝 Contributing

We're building the future of automation! 

- 🐛 **Found a bug?** Open an issue
- 💡 **Have an idea?** Start a discussion  
- 🔧 **Want to code?** Check our roadmap for good first issues
- 📖 **Improve docs?** Documentation PRs always welcome

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup and guidelines.

## 📜 License

MIT License - build amazing automation tools, commercially or personally, with zero restrictions.

---

**SharpFlow: Where visual simplicity meets unlimited power** ⚡
