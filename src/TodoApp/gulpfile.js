/// <binding ProjectOpened='watch' />
const browserify = require('browserify');
const buffer = require('vinyl-buffer');
const eslint = require('gulp-eslint');
const gulp = require('gulp');
const jest = require('gulp-jest').default;
const prettier = require('gulp-prettier');
const source = require('vinyl-source-stream');
const sourcemaps = require('gulp-sourcemaps');
const tsify = require('tsify');
const uglify = require('gulp-uglify');
const sourceFiles = ['scripts/ts/**/*.ts'];

gulp.task('prettier', function () {
    return gulp.src(sourceFiles)
        .pipe(prettier())
        .pipe(gulp.dest(file => file.base))
        .end();
});

gulp.task('lint', function () {
    return gulp.src(sourceFiles)
        .pipe(eslint({
            formatter: 'visualstudio'
        }))
        .pipe(eslint.format())
        .pipe(eslint.failAfterError());
});

gulp.task('build', function () {
    return browserify({
        basedir: '.',
        debug: true,
        entries: ['scripts/ts/main.ts'],
        cache: {},
        packageCache: {}
    })
        .plugin(tsify)
        .transform('babelify', {
            presets: ['@babel/preset-env'],
            extensions: ['.ts']
        })
        .bundle()
        .pipe(source('main.js'))
        .pipe(buffer())
        .pipe(sourcemaps.init({ loadMaps: true }))
        .pipe(uglify())
        .pipe(sourcemaps.write('./'))
        .pipe(gulp.dest('wwwroot/static/js'));
});

gulp.task('test', function () {
    return gulp.src('scripts')
        .pipe(jest({
            collectCoverage: true
        }));
});

gulp.task('default', gulp.series('prettier', 'lint', 'build', 'test'));

gulp.task('watch', function () {
    gulp.watch(sourceFiles, gulp.series('lint', 'build', 'test'));
});
