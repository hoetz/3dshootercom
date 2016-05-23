var gulp = require('gulp');
var concat = require('gulp-concat');
var watch = require('gulp-watch');

var config = {
    //Include all js files but exclude any min.js files
    src: [
            'bower_components/jquery/dist/jquery.js',
            'bower_components/jquery-validation/dist/jquery.validate.min.js',
            'bower_components/handlebars/handlebars.js',
            'bower_components/bootstrap/dist/js/bootstrap.js',
            'scripts/*.js',
            'scripts/**/*.js'
    ],
    srcCss: ['Content/stylesheets/*.css'],
    rootFolder:"wwwroot/dist/app"
}

gulp.task('default', ['scripts', 'BootstrapFontCss'], function () {

});


gulp.task('scripts', function () {
    return gulp.src(config.src
        ,
        { base: 'bower_components/' })
      .pipe(concat('main.js'))
      .pipe(gulp.dest(config.rootFolder + '/js'));
});


gulp.task('BootstrapFontCss', function () {
    gulp.src('Content/stylesheets/*.{ttf,woff,woff2,eof,eot,svg}')
    .pipe(gulp.dest(config.rootFolder+'/css'));

    gulp.src(['Content/stylesheets/*.css'])
    .pipe(concat('styles.css'))
    .pipe(gulp.dest(config.rootFolder+'/css'));
});

gulp.task('watch', function () {
    //gulp.start('aspnet-run');
    gulp.watch([config.src], ['scripts']);
    gulp.watch([config.srcCss], ['BootstrapFontCss']);

});
