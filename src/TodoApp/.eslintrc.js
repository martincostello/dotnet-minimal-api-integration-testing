module.exports = {
    'env': {
        'browser': true,
        'es6': true,
        'node': true
    },
    'extends': [
        'prettier'
    ],
    'parser': '@typescript-eslint/parser',
    'parserOptions': {
        'sourceType': 'module'
    },
    'plugins': [
        '@typescript-eslint'
    ],
    'rules': {
        '@typescript-eslint/indent': 'error',
        '@typescript-eslint/member-delimiter-style': 'error',
        '@typescript-eslint/naming-convention': 'error',
        '@typescript-eslint/prefer-namespace-keyword': 'error',
        '@typescript-eslint/quotes': [
            'error',
            'single'
        ],
        '@typescript-eslint/semi': [
            'error',
            'always'
        ],
        '@typescript-eslint/type-annotation-spacing': 'error',
        'brace-style': [
            'error',
            '1tbs'
        ],
        'eqeqeq': [
            'error',
            'smart'
        ],
        'id-blacklist': [
            'error',
            'any',
            'Number',
            'number',
            'String',
            'string',
            'Boolean',
            'boolean',
            'Undefined',
            'undefined'
        ],
        'id-match': 'error',
        'no-eval': 'error',
        'no-redeclare': 'error',
        'no-trailing-spaces': 'error',
        'no-underscore-dangle': 'error',
        'no-var': 'error',
        'spaced-comment': [
            'error',
            'always',
            {
                'markers': [
                    '/'
                ]
            }
        ]
    }
};
