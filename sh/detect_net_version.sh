#!/bin/bash

# Function to detect .NET target framework from a project file
detect_target_framework() {
    local project_file="$1"
    if [ ! -f "$project_file" ]; then
        echo "Error: Project file $project_file not found" >&2
        return 1
    fi
    
    # Extract TargetFramework from the project file
    local target_framework=$(grep -o '<TargetFramework>[^<]*</TargetFramework>' "$project_file" | sed 's/<TargetFramework>//;s/<\/TargetFramework>//')
    
    if [ -z "$target_framework" ]; then
        echo "Error: Could not detect TargetFramework in $project_file" >&2
        return 1
    fi
    
    echo "$target_framework"
    return 0
}

# Function to get the build output directory
get_build_output_dir() {
    local project_file="$1"
    local target_framework=$(detect_target_framework "$project_file")
    
    if [ $? -ne 0 ]; then
        return 1
    fi
    
    # Extract project directory and build the output path
    local project_dir=$(dirname "$project_file")
    echo "$project_dir/bin/Debug/$target_framework"
    return 0
}

# Function to get the allure results directory
get_allure_results_dir() {
    local project_file="$1"
    local build_output_dir=$(get_build_output_dir "$project_file")
    
    if [ $? -ne 0 ]; then
        return 1
    fi
    
    echo "$build_output_dir/allure-results"
    return 0
}
