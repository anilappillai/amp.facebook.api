# ---------------------------------------------------------------------------
# AWS Secrets Manager secret — holds ALL app configuration.
# The actual secret value is set outside Terraform (manually or via CI/CD)
# so it is NEVER stored in state files or source control.
#
# Secret JSON structure (set once via AWS Console / CLI):
# {
#   "ConnectionStrings__Optima2Connection": "Server=...;Database=...;...",
#   "ConnectionStrings__SalesRepConnection": "Server=...;Database=...;...",
#   "Serilog__WriteTo__0__Args__logGroupName": "/aws/eks/amp-facebook-api"
# }
# ---------------------------------------------------------------------------
resource "aws_secretsmanager_secret" "amp_facebook_api_config" {
  name        = "amp-facebook-api/${var.environment}"
  description = "All configuration secrets for Amp.Facebook.Api (${var.environment})"

  tags = merge(var.common_tags, {
    Environment = var.environment
  })
}

# Rotation is recommended for database credentials — configure here when ready
# resource "aws_secretsmanager_secret_rotation" "amp_facebook_api_config" { ... }

output "secret_arn" {
  description = "ARN of the Secrets Manager secret"
  value       = aws_secretsmanager_secret.amp_facebook_api_config.arn
}

output "ecr_repository_url" {
  description = "ECR repository URL for pushing Docker images"
  value       = aws_ecr_repository.amp_facebook_api.repository_url
}

output "iam_role_arn" {
  description = "IAM role ARN for IRSA annotation on the Kubernetes ServiceAccount"
  value       = aws_iam_role.amp_facebook_api.arn
}
