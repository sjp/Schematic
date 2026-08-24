import { RouterProvider } from "@tanstack/react-router";

import { useColorScheme } from "@/hooks/useColorScheme";
import { router } from "@/router";

export default function App() {
  useColorScheme();
  return <RouterProvider router={router} />;
}
