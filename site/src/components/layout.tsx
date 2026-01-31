import { Link, useLocation } from "react-router-dom";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Moon, Sun, Github, ChevronRight } from "lucide-react";
import { useState, useEffect } from "react";

const navigation = [
  { name: "Home", href: "/" },
  { name: "Getting Started", href: "/getting-started" },
  { name: "Template Engine", href: "/template-engine" },
  { name: "Variable Resolver", href: "/variable-resolver" },
  { name: "Execution Context", href: "/execution-context" },
  { name: "Examples", href: "/examples" },
  { name: "Code Generation", href: "/code-generation" },
  { name: "API Reference", href: "/api-reference" },
];

export function Layout({ children }: { children: React.ReactNode }) {
  const location = useLocation();
  const [isDark, setIsDark] = useState(() => {
    const saved = localStorage.getItem("theme");
    if (saved) return saved === "dark";
    return true; // Default to dark mode
  });

  const currentPage = navigation.find(item => item.href === location.pathname);
  const isHomePage = location.pathname === "/";

  useEffect(() => {
    const root = document.documentElement;
    if (isDark) {
      root.classList.add("dark");
      localStorage.setItem("theme", "dark");
    } else {
      root.classList.remove("dark");
      localStorage.setItem("theme", "light");
    }
  }, [isDark]);

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="flex h-14 items-center px-4 gap-4">
          <Link to="/" className="flex items-center space-x-2">
            <div className="h-8 w-8 rounded-lg bg-primary flex items-center justify-center">
              <span className="text-primary-foreground font-bold text-sm">LT</span>
            </div>
            <span className="font-bold text-lg">
              <span className="text-primary">Tiny</span>Tools
            </span>
          </Link>

          {/* Breadcrumb */}
          {currentPage && currentPage.href !== "/" && (
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <ChevronRight className="h-4 w-4" />
              <span className="font-medium text-foreground">{currentPage.name}</span>
            </div>
          )}

          <div className="flex items-center ml-auto space-x-2">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setIsDark(!isDark)}
            >
              {isDark ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
            </Button>
            <Button variant="ghost" size="icon" asChild>
              <a
                href="https://github.com/lowlandtech/tinytools"
                target="_blank"
                rel="noopener noreferrer"
              >
                <Github className="h-5 w-5" />
              </a>
            </Button>
          </div>
        </div>
      </header>

      <div className="flex">
        {/* Left Sidebar - Hidden on home page */}
        {!isHomePage && (
          <aside className="sticky top-14 h-[calc(100vh-3.5rem)] w-64 border-r bg-sidebar overflow-y-auto">
            <nav className="flex flex-col gap-1 p-4">
              {navigation.map((item) => (
                <Link
                  key={item.href}
                  to={item.href}
                  className={cn(
                    "px-3 py-2 rounded-md text-sm font-medium transition-colors",
                    location.pathname === item.href
                      ? "bg-sidebar-accent text-sidebar-accent-foreground"
                      : "text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
                  )}
                >
                  {item.name}
                </Link>
              ))}
            </nav>
          </aside>
        )}

        {/* Main Content */}
        <main className="flex-1 overflow-y-auto">
          <div className={cn(
            "container mx-auto px-4 py-6",
            isHomePage ? "max-w-6xl" : "max-w-5xl"
          )}>
            {children}
          </div>
        </main>
      </div>

      {/* Footer */}
      <footer className="border-t py-6 md:py-0">
        <div className={cn(
          "container mx-auto flex flex-col items-center justify-between gap-4 px-4 md:h-16 md:flex-row",
          !isHomePage && "ml-64"
        )}>
          <p className="text-sm text-muted-foreground">
            © {new Date().getFullYear()} LowlandTech. All rights reserved.
          </p>
          <p className="text-sm text-muted-foreground">
            Built with React, shadcn/ui, and Tailwind CSS
          </p>
        </div>
      </footer>
    </div>
  );
}
