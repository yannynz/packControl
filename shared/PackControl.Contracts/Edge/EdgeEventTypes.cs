namespace PackControl.Contracts.Edge;

public static class EdgeEventTypes
{
    public const string FileDetected = "edge.file.detected";
    public const string PdfImported = "edge.order.pdf_imported";
    public const string DxfAnalysisRequested = "edge.dxf.analysis.requested";
    public const string ProductionSignalReceived = "edge.production.signal.received";
}
