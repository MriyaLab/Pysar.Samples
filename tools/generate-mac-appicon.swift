#!/usr/bin/env swift

// Generates AppIcon.icns, the macOS app icon for the Mac Catalyst build.
//
// MAUI's Resizetizer only emits an iOS asset catalog, and an icon referenced through the asset
// catalog (CFBundleIconName) makes macOS draw its "iOS app on Mac" decoration: the artwork is
// shrunk onto a light rounded plate, so the icon reads as light grey instead of black. A classic
// .icns referenced by CFBundleIconFile is rendered as-is, which means the artwork has to carry the
// rounded corners and the icon-grid margin itself.
//
// Usage (from the repository root):
//   swift tools/generate-mac-appicon.swift

import AppKit

// macOS icon grid: a 1024pt canvas with 100pt of margin on every side, corners rounded to ~22.37%
// of the artwork edge.
let canvasSize: CGFloat = 1024
let artworkMargin: CGFloat = 100
let artworkSize = canvasSize - artworkMargin * 2
let cornerRadius = artworkSize * 0.2237

let repositoryRoot = URL(fileURLWithPath: FileManager.default.currentDirectoryPath)
let iconSources = repositoryRoot.appendingPathComponent("src/Pysar.Maui.Sample/Resources/AppIcon")
let backgroundUrl = iconSources.appendingPathComponent("appicon.svg")
let foregroundUrl = iconSources.appendingPathComponent("mac_appiconfg.svg")
let outputUrl = repositoryRoot.appendingPathComponent(
    "src/Pysar.Maui.Sample/Platforms/MacCatalyst/Resources/AppIcon.icns")

/// The point size and scale of one representation inside the .icns, and the pixel size and
/// `iconutil` file name that follow from them.
struct IconVariant {
    let pointSize: Int
    let scale: Int

    var pixelSize: Int { pointSize * scale }
    var fileName: String {
        let suffix = scale == 1 ? "" : "@\(scale)x"
        return "icon_\(pointSize)x\(pointSize)\(suffix).png"
    }
}

let variants = [16, 32, 128, 256, 512].flatMap { pointSize in
    [1, 2].map { IconVariant(pointSize: pointSize, scale: $0) }
}

func fail(_ message: String) -> Never {
    FileHandle.standardError.write("\(message)\n".data(using: .utf8)!)
    exit(1)
}

func loadImage(_ url: URL) -> NSImage {
    guard let image = NSImage(contentsOf: url) else { fail("Cannot read \(url.path)") }
    return image
}

func render(width: Int, height: Int, _ draw: () -> Void) -> NSBitmapImageRep {
    let rep = NSBitmapImageRep(
        bitmapDataPlanes: nil, pixelsWide: width, pixelsHigh: height,
        bitsPerSample: 8, samplesPerPixel: 4, hasAlpha: true, isPlanar: false,
        colorSpaceName: .deviceRGB, bytesPerRow: 0, bitsPerPixel: 0)!
    rep.size = NSSize(width: width, height: height)

    let context = NSGraphicsContext(bitmapImageRep: rep)!
    NSGraphicsContext.saveGraphicsState()
    NSGraphicsContext.current = context
    context.imageInterpolation = .high
    draw()
    NSGraphicsContext.restoreGraphicsState()

    return rep
}

// The master is rendered once at full resolution; the smaller variants are downscaled from it so
// every size shares the same geometry.
let background = loadImage(backgroundUrl)
let foreground = loadImage(foregroundUrl)
let artworkRect = NSRect(x: artworkMargin, y: artworkMargin, width: artworkSize, height: artworkSize)

let masterRep = render(width: Int(canvasSize), height: Int(canvasSize)) {
    NSBezierPath(roundedRect: artworkRect, xRadius: cornerRadius, yRadius: cornerRadius).setClip()
    background.draw(in: artworkRect)
    foreground.draw(in: artworkRect)
}

let master = NSImage(size: NSSize(width: canvasSize, height: canvasSize))
master.addRepresentation(masterRep)

let iconSet = FileManager.default.temporaryDirectory
    .appendingPathComponent("Pysar-\(UUID().uuidString)/AppIcon.iconset")
try! FileManager.default.createDirectory(at: iconSet, withIntermediateDirectories: true)
defer { try? FileManager.default.removeItem(at: iconSet.deletingLastPathComponent()) }

for variant in variants {
    let rep = render(width: variant.pixelSize, height: variant.pixelSize) {
        master.draw(in: NSRect(x: 0, y: 0, width: variant.pixelSize, height: variant.pixelSize))
    }
    guard let data = rep.representation(using: .png, properties: [:]) else {
        fail("Cannot encode \(variant.fileName)")
    }
    try! data.write(to: iconSet.appendingPathComponent(variant.fileName))
}

let iconutil = Process()
iconutil.executableURL = URL(fileURLWithPath: "/usr/bin/iconutil")
iconutil.arguments = ["--convert", "icns", iconSet.path, "--output", outputUrl.path]
try! iconutil.run()
iconutil.waitUntilExit()

guard iconutil.terminationStatus == 0 else { fail("iconutil failed") }

print("Wrote \(outputUrl.path)")
