import stylistic from "@stylistic/eslint-plugin";
import typescriptEslint from "@typescript-eslint/eslint-plugin";
import jest from "eslint-plugin-jest";
import globals from "globals";
import tsParser from "@typescript-eslint/parser";
import path from "node:path";
import { fileURLToPath } from "node:url";
import js from "@eslint/js";
import { FlatCompat } from "@eslint/eslintrc";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const compat = new FlatCompat({
    baseDirectory: __dirname,
    recommendedConfig: js.configs.recommended,
    allConfig: js.configs.all
});

export default [...compat.extends("prettier"), {
    files: ["**/*.ts"],
    ignores: [
        "bin/",
        "node_modules/",
        "obj/",
        "wwwroot/"
    ],
    plugins: {
        "@stylistic": stylistic,
        "@typescript-eslint": typescriptEslint,
        jest,
    },
    languageOptions: {
        globals: {
            ...globals.browser,
            ...jest.environments.globals.globals,
            ...globals.node,
        },
        parser: tsParser,
        ecmaVersion: 5,
        sourceType: "module",
        parserOptions: {
            project: "./tsconfig.json",
        },
    },
    rules: {
        "@stylistic/indent": "error",
        "@stylistic/member-delimiter-style": "error",
        "@stylistic/quotes": ["error", "single"],
        "@stylistic/semi": ["error", "always"],
        "@stylistic/type-annotation-spacing": "error",
        "@typescript-eslint/naming-convention": "error",
        "@typescript-eslint/prefer-namespace-keyword": "error",
        "brace-style": ["error", "1tbs"],
        eqeqeq: ["error", "smart"],
        "id-blacklist": [
            "error",
            "any",
            "Number",
            "number",
            "String",
            "string",
            "Boolean",
            "boolean",
            "Undefined",
            "undefined",
        ],
        "id-match": "error",
        "no-eval": "error",
        "no-redeclare": "error",
        "no-trailing-spaces": "error",
        "no-underscore-dangle": "error",
        "no-var": "error",
        "spaced-comment": ["error", "always", {
            markers: ["/"],
        }],
    },
}];
