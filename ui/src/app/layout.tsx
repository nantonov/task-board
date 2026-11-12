import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Task board",
  description: "What the Outstaff kit is doing, card by card",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
