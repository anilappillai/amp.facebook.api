terraform {
  required_version = ">= 1.6"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  # Store state in S3 — update bucket/key per environment
  backend "s3" {
    bucket         = "your-org-terraform-state"
    key            = "amp-facebook-api/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "terraform-state-lock"
  }
}

provider "aws" {
  region = var.aws_region
}

# ---------------------------------------------------------------------------
# ECR — container image registry
# ---------------------------------------------------------------------------
resource "aws_ecr_repository" "amp_facebook_api" {
  name                 = "amp-facebook-api"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = var.common_tags
}

resource "aws_ecr_lifecycle_policy" "amp_facebook_api" {
  repository = aws_ecr_repository.amp_facebook_api.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Keep last 10 images"
      selection = {
        tagStatus   = "any"
        countType   = "imageCountMoreThan"
        countNumber = 10
      }
      action = { type = "expire" }
    }]
  })
}

# ---------------------------------------------------------------------------
# IRSA — IAM Role for the EKS pod Service Account
# The pod reads secrets from Secrets Manager using this role.
# No static credentials anywhere.
# ---------------------------------------------------------------------------
data "aws_eks_cluster" "cluster" {
  name = var.eks_cluster_name
}

data "aws_iam_openid_connect_provider" "eks" {
  url = data.aws_eks_cluster.cluster.identity[0].oidc[0].issuer
}

resource "aws_iam_role" "amp_facebook_api" {
  name = "amp-facebook-api-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Principal = {
        Federated = data.aws_iam_openid_connect_provider.eks.arn
      }
      Action = "sts:AssumeRoleWithWebIdentity"
      Condition = {
        StringEquals = {
          "${replace(data.aws_iam_openid_connect_provider.eks.url, "https://", "")}:sub" =
            "system:serviceaccount:${var.kubernetes_namespace}:amp-facebook-api-sa"
        }
      }
    }]
  })

  tags = var.common_tags
}

resource "aws_iam_role_policy" "amp_facebook_api_secrets" {
  name = "amp-facebook-api-secrets-policy"
  role = aws_iam_role.amp_facebook_api.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect   = "Allow"
      Action   = ["secretsmanager:GetSecretValue"]
      Resource = aws_secretsmanager_secret.amp_facebook_api_config.arn
    }]
  })
}
