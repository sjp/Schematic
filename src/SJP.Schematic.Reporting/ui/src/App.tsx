import { RouterProvider } from "@tanstack/react-router";
import { router } from "@/router";
import { useColorScheme } from "@/hooks/useColorScheme";

export default function App() {
  useColorScheme();
  return <RouterProvider router={router} />;
}
