using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Data;

[GalleryPage("Code snippet", GalleryCategory.Data, Order = 20, Spec = "CodeSnippet",
    Summary = "Displays code or a command inline, as a copyable line, or as a multi-line block that can expand.")]
public partial class CodeSnippetPage : GalleryPage
{
    const string Curl = "curl -X POST \"https://api.example.cloud/v1/clusters/prod-us-east-1/workers\" -H \"Authorization: Bearer $API_KEY\" -H \"Content-Type: application/json\" -d '{\"count\": 3, \"flavor\": \"bx2.4x16\", \"zone\": \"us-east-2\"}'";

    static readonly string Yaml = string.Join('\n',
        "apiVersion: apps/v1", "kind: Deployment", "metadata:", "  name: checkout-api", "  namespace: payments",
        "  labels:", "    app: checkout-api", "spec:", "  replicas: 3", "  selector:", "    matchLabels:",
        "      app: checkout-api", "  template:", "    metadata:", "      labels:", "        app: checkout-api",
        "    spec:", "      containers:", "        - name: checkout-api",
        "          image: registry.example.cloud/payments/checkout-api:2.14.0", "          ports:",
        "            - containerPort: 8080", "          resources:", "            limits:",
        "              cpu: \"500m\"", "              memory: 512Mi");

    static readonly string Env = string.Join('\n',
        "export CLUSTER=prod-us-east-1", "export REGION=us-east", "export KUBECONFIG=$HOME/.kube/prod-us-east-1.yaml",
        "# Tokens expire after 24 hours; run \"cloud login\" again to refresh the kubeconfig file.");

    static readonly string Nodes = string.Join('\n',
        "NAME         STATUS   ROLES    AGE   VERSION", "10.240.0.12  Ready    <none>   41d   v1.29.4",
        "10.240.0.13  Ready    <none>   41d   v1.29.4", "10.240.0.14  NotReady <none>   2m    v1.29.4");

    public CodeSnippetPage()
    {
        InitializeComponent();
        LongSingle.Code = Curl;
        Manifest.Code = Yaml;
        ExpandedManifest.Code = Yaml;
        Wrapped.Code = Env;
        Output.Code = Nodes;
        DisabledMulti.Code = "cloud cluster stop dev-jp-tok\ncloud cluster delete dev-jp-tok --force";
    }

    void OnCopied(object? sender, EventArgs e) => Status.Text = "Copied the cluster configuration command";
}
